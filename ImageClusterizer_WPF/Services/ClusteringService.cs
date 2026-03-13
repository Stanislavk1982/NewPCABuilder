using System.Numerics;
using ImageClusterizer_WPF.Models;

namespace ImageClusterizer_WPF.Services;

public class ClusteringService
{
      public static float CosineSimilarity(float[] a, float[] b)
      {
                int n = a.Length, step = Vector<float>.Count;
                var vDot = Vector<float>.Zero; var vNA = Vector<float>.Zero; var vNB = Vector<float>.Zero;
                int i = 0;
                for (; i <= n - step; i += step) { var va = new Vector<float>(a,i); var vb = new Vector<float>(b,i); vDot+=va*vb; vNA+=va*va; vNB+=vb*vb; }
                float dot=Vector.Dot(vDot,Vector<float>.One), nA=Vector.Dot(vNA,Vector<float>.One), nB=Vector.Dot(vNB,Vector<float>.One);
                for (; i<n; i++) { dot+=a[i]*b[i]; nA+=a[i]*a[i]; nB+=b[i]*b[i]; }
                return dot / (MathF.Sqrt(nA*nB) + 1e-10f);
      }

      public List<ImageCluster> ClusterBySimilarity(List<ImageVector> vectors, float threshold=0.85f)
      {
                int n = vectors.Count; var visited = new bool[n]; var clusters = new List<ImageCluster>();
                for (int i=0; i<n; i++) {
                              if (visited[i]) continue;
                              var c = new ImageCluster { ClusterId=clusters.Count, Images=new(){vectors[i]} }; visited[i]=true;
                              for (int j=i+1; j<n; j++) { if (!visited[j] && CosineSimilarity(vectors[i].Vector,vectors[j].Vector)>=threshold) { c.Images.Add(vectors[j]); visited[j]=true; } }
                              clusters.Add(c);
                }
                return clusters;
      }

      public static float[] ToSparse(float[] vector, int sparseTopN)
      {
                if (sparseTopN<=0 || sparseTopN>=vector.Length) return vector;
                var result = new float[vector.Length];
                foreach (var idx in vector.Select((v,i)=>(i,MathF.Abs(v))).OrderByDescending(x=>x.Item2).Take(sparseTopN).Select(x=>x.i))
                              result[idx]=vector[idx];
                return result;
      }

      public List<ClusterPosition> CalculatePositions(List<ImageCluster> clusters, int w, int h)
                => CalculatePositionsSparse(clusters, w, h, int.MaxValue);

      public List<ClusterPosition> CalculatePositionsSparse(List<ImageCluster> clusters, int w, int h, int sparseTopN, IProgress<(int,int,string)>? progress=null)
      {
                var allImages = clusters.SelectMany(c=>c.Images).ToList(); int n=allImages.Count;
                if (n==0) return new();
                progress?.Report((0,4,"Compressing vectors..."));
                var cv = allImages.Select(iv=>ToSparse(iv.Vector,sparseTopN)).ToList();
                int d = cv[0].Length;
                progress?.Report((1,4,$"PCA: {n}x{d} matrix..."));
                var X = new float[n,d]; for (int i=0;i<n;i++) for (int j=0;j<d;j++) X[i,j]=cv[i][j];
                progress?.Report((2,4,"Running Randomized SVD..."));
                var reduced = ReduceTo2D(X,n,d);
                progress?.Report((3,4,"Normalizing..."));
                var norm = Normalize(reduced,n,w,h);
                var positions = new List<ClusterPosition>();
                for (int i=0;i<n;i++) { var cl=clusters.First(c=>c.Images.Contains(allImages[i])); positions.Add(new(){Image=allImages[i],X=norm[i,0],Y=norm[i,1],ClusterId=cl.ClusterId}); }
                foreach (var c in clusters) { var cp=positions.Where(p=>p.ClusterId==c.ClusterId).ToList(); if (!cp.Any()) continue; double cx=cp.Average(p=>p.X),cy=cp.Average(p=>p.Y); var cent=cp.MinBy(p=>Math.Pow(p.X-cx,2)+Math.Pow(p.Y-cy,2)); if(cent!=null) cent.IsCentroid=true; }
                progress?.Report((4,4,$"Done — {n} images."));
                return positions;
      }

      private static float[,] ReduceTo2D(float[,] X, int n, int d)
      {
                int k=2,l=k+10; var rng=new Random(42);
                var means=new float[d]; for(int j=0;j<d;j++){float s=0;for(int i=0;i<n;i++)s+=X[i,j];means[j]=s/n;}
                var Xc=new float[n,d]; for(int i=0;i<n;i++) for(int j=0;j<d;j++) Xc[i,j]=X[i,j]-means[j];
                var Omega=RandG(d,l,rng); var Y=Mul(Xc,Omega,n,d,l);
                for(int q=0;q<2;q++){var Z=MulTrans(Xc,Y,n,d,l);Y=Mul(Xc,Z,n,d,l);}
                var Q=GS(Y,n,l); var B=MulQtX(Q,Xc,n,d,l); var Vt=SVDApprox(B,l,d,k); var V=Trans(Vt,k,d);
                return Mul(Xc,V,n,d,k);
      }

      private static float[,] Normalize(float[,] r,int n,int w,int h)
      {
                float minX=float.MaxValue,maxX=float.MinValue,minY=float.MaxValue,maxY=float.MinValue;
                for(int i=0;i<n;i++){if(r[i,0]<minX)minX=r[i,0];if(r[i,0]>maxX)maxX=r[i,0];if(r[i,1]<minY)minY=r[i,1];if(r[i,1]>maxY)maxY=r[i,1];}
                float rx=maxX-minX+1e-10f,ry=maxY-minY+1e-10f; const float m=0.05f;
                var res=new float[n,2];
                for(int i=0;i<n;i++){res[i,0]=(r[i,0]-minX)/rx*(w*(1-2*m))+w*m;res[i,1]=(r[i,1]-minY)/ry*(h*(1-2*m))+h*m;}
                return res;
      }

      private static float[,] RandG(int rows,int cols,Random rng){var M=new float[rows,cols];for(int i=0;i<rows;i++)for(int j=0;j<cols;j++){double u1=1-rng.NextDouble(),u2=1-rng.NextDouble();M[i,j]=(float)(Math.Sqrt(-2*Math.Log(u1))*Math.Cos(2*Math.PI*u2));}return M;}
      private static float[,] Mul(float[,] A,float[,] B,int n,int d,int l){var C=new float[n,l];Parallel.For(0,n,i=>{for(int k=0;k<d;k++){float a=A[i,k];for(int j=0;j<l;j++)C[i,j]+=a*B[k,j];}});return C;}
      private static float[,] MulTrans(float[,] Xc,float[,] Y,int n,int d,int l){var Z=new float[d,l];for(int k=0;k<n;k++)for(int j=0;j<d;j++)for(int jj=0;jj<l;jj++)Z[j,jj]+=Xc[k,j]*Y[k,jj];return Z;}
      private static float[,] MulQtX(float[,] Q,float[,] Xc,int n,int d,int l){var B=new float[l,d];for(int i=0;i<l;i++)for(int j=0;j<d;j++)for(int k=0;k<n;k++)B[i,j]+=Q[k,i]*Xc[k,j];return B;}
      private static float[,] GS(float[,] Y,int n,int l){var Q=new float[n,l];for(int j=0;j<l;j++){var col=new float[n];for(int i=0;i<n;i++)col[i]=Y[i,j];for(int k=0;k<j;k++){float dot=0;for(int i=0;i<n;i++)dot+=Q[i,k]*col[i];for(int i=0;i<n;i++)col[i]-=dot*Q[i,k];}float nm=MathF.Sqrt(col.Sum(x=>x*x));if(nm>1e-10f)for(int i=0;i<n;i++)Q[i,j]=col[i]/nm;}return Q;}
      private static float[,] SVDApprox(float[,] B,int l,int d,int k){var Vt=new float[k,d];for(int i=0;i<k&&i<l;i++)for(int j=0;j<d;j++)Vt[i,j]=B[i,j];for(int i=0;i<k;i++){for(int p=0;p<i;p++){float dot=0;for(int j=0;j<d;j++)dot+=Vt[i,j]*Vt[p,j];for(int j=0;j<d;j++)Vt[i,j]-=dot*Vt[p,j];}float nm=MathF.Sqrt(Enumerable.Range(0,d).Sum(j=>(double)(Vt[i,j]*Vt[i,j])));if(nm>1e-10)for(int j=0;j<d;j++)Vt[i,j]/=(float)nm;}return Vt;}
      private static float[,] Trans(float[,] M,int rows,int cols){var T=new float[cols,rows];for(int i=0;i<rows;i++)for(int j=0;j<cols;j++)T[j,i]=M[i,j];return T;}
}
