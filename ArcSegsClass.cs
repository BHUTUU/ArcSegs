using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;

namespace ArcSegs
{
    public class ArcSegsClass
    {
        [CommandMethod("ARCSEGS")]
        public void ArcSegs()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            Editor ed = doc.Editor;
            PromptIntegerOptions pio = new PromptIntegerOptions("\nEnter number of segments per unit length : ");
            pio.DefaultValue = 1;
            pio.AllowNegative = false;
            pio.AllowZero = false;
            PromptIntegerResult pir = ed.GetInteger(pio);
            if (pir.Status != PromptStatus.OK) return;
            int numSegsPerUnit = pir.Value;
            CreateArcSegments(false, numSegsPerUnit);
        }

        [CommandMethod("ARCSEGS2")]
        public void ArcSegs2()
        {
            CreateArcSegments(true, 2);
        }
        public void CreateArcSegments(bool use_segments_per_unit, int number_of_segs)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            TypedValue[] tvs = { new TypedValue((int)DxfCode.Start, "ARC,CIRCLE,SPLINE") };
            SelectionFilter filter = new SelectionFilter(tvs);
            PromptSelectionOptions pso = new PromptSelectionOptions();
            pso.MessageForAdding ="\nSelect arc/circle/spline : ";
            PromptSelectionResult psr = ed.GetSelection(pso, filter);
            if (psr.Status != PromptStatus.OK) return;
            using (DocumentLock docLock = doc.LockDocument())
            {
                using (Transaction tr =db.TransactionManager.StartTransaction())
                {
                    BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                    BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                    foreach (SelectedObject so in psr.Value)
                    {
                        if (so == null) continue;
                        Entity ent = tr.GetObject(so.ObjectId, OpenMode.ForWrite) as Entity;
                        if (ent == null) continue;
                        Curve curve = ent as Curve;
                        if (curve == null) continue;
                        double length;
                        try
                        {
                            length = curve.GetDistanceAtParameter(curve.EndParam);
                        }
                        catch
                        {
                            continue;
                        }
                        int num;
                        if (use_segments_per_unit)
                        {
                            num = Math.Max(1, (int)(length * number_of_segs));
                        }
                        else
                        {
                            num = Math.Max(1, (number_of_segs));
                        }
                        double inc = length / num;
                        List<Point3d> pts = new List<Point3d>();
                        for (int i = 0; i <= num; i++)
                        {
                            double dist = i * inc;
                            if (dist > length) dist = length;
                            try
                            {
                                Point3d pt = curve.GetPointAtDist(dist);
                                pts.Add(pt);
                            }
                            catch { }
                        }
                        try
                        {
                            Point3d endPt = curve.EndPoint;
                            if (pts.Count == 0 || pts[pts.Count - 1].DistanceTo(endPt) > 1e-6)
                            {
                                pts.Add(endPt);
                            }
                        }
                        catch { }
                        if (pts.Count < 2) continue;
                        Polyline pl = new Polyline();
                        for (int i = 0; i < pts.Count; i++)
                        {
                            Point3d pt = pts[i];
                            pl.AddVertexAt(i, new Point2d(pt.X, pt.Y), 0, 0, 0);
                        }
                        pl.Closed = curve.Closed;
                        pl.LayerId = ent.LayerId;
                        pl.Color = ent.Color;
                        pl.LinetypeId = ent.LinetypeId;
                        pl.LinetypeScale = ent.LinetypeScale;
                        pl.LineWeight = ent.LineWeight;
                        ms.AppendEntity(pl);
                        tr.AddNewlyCreatedDBObject(pl, true);
                        ent.Erase();
                    }
                    tr.Commit();
                }
            }
            ed.WriteMessage("\nDone.");
        }
    }
}