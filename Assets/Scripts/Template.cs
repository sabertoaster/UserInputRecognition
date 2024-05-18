using System.Collections;
using System.Collections.Generic;
using System;
using System.Xml.Linq;
using UnityEngine;
using System.Linq;

public class Template
{
    public string XMLpath = "";
    public XDocument doc = new XDocument(new XElement("Gesture"));
    public int nSamples;
    public float rescaleSize;
    public List<Vector2> templatePoints;
    public Vector2 startPos;
    public Vector2 endPos;
    public Template(string nameFile, int samples, float rcSize, List<Vector2> Points)
    {
        XMLpath = nameFile + ".xml";
        nSamples = samples;
        rescaleSize = rcSize;
        templatePoints = Points;
        startPos = Points[0];
        endPos = Points[^1];
    }
    public void saveToXML(string folderPth)
    {
        XMLpath = folderPth + "\\" + XMLpath;
        setSamples(nSamples);
        setSize(rescaleSize, templatePoints);
        setTemplatePoints(templatePoints);
        setStartEnd(startPos, endPos);
        doc.Save(XMLpath);
    }
    public void setSamples(int nSamples)
    {
        XElement sample = new XElement("Sample");
        sample.SetAttributeValue("Samples", nSamples);
        doc.Root.Add(sample);
    }

    public void setSize(float rescaleSize, List<Vector2> templatePoints)
    {
        //List<Vector2> bbox = new List<Vector2>();
        //Vector2 minVector = new Vector2(0, 0);
        //Vector2 maxVector = new Vector2(0, 0);
        //foreach (Vector2 point in templatePoints)
        //{
        //    if ((minVector.x == 0) || (point.x < minVector.x))
        //    {
        //        minVector.x = point.x;
        //    }
        //    if ((minVector.y == 0) || (point.y < minVector.x))
        //    {
        //        minVector.y = point.y;
        //    }
        //    if ((maxVector.x == 0) || (point.x > maxVector.x))
        //    {
        //        maxVector.x = point.x;
        //    }
        //    if ((maxVector.y == 0) || (point.y > maxVector.x))
        //    {
        //        maxVector.y = point.y;
        //    }
        //}
        //bbox.Add(new Vector2(minVector.x, maxVector.y));
        //bbox.Add(new Vector2(maxVector.x, minVector.y));
        //rescaleSize = ((bbox[0] - bbox[1]).magnitude) / Mathf.Sqrt(2);
        XElement size = new XElement("Size");
        size.SetAttributeValue("Size", rescaleSize);
        doc.Root.Add(size);
    }

    public void setTemplatePoints(List<Vector2> templatePoints)
    {
        XElement template = new XElement("Templates");
        foreach (Vector2 point in templatePoints)
        {
            XElement p = new XElement("Point");
            p.SetAttributeValue("x", point.x);
            p.SetAttributeValue("y", point.y);
            template.Add(p);
        }
        doc.Root.Add(template);
    }

    public void setStartEnd(Vector2 startPos, Vector2 endPos)
    {
        XElement start_end = new XElement("Start_End");
        XElement start = new XElement("Start");
        start.SetAttributeValue("StartPos_x", startPos.x);
        start.SetAttributeValue("StartPos_y", startPos.y);
        XElement end = new XElement("End");
        end.SetAttributeValue("EndPos_x", endPos.x);
        end.SetAttributeValue("EndPos_y", endPos.y);
        start_end.Add(start);
        start_end.Add(end);
        doc.Root.Add(start_end);
    }
}