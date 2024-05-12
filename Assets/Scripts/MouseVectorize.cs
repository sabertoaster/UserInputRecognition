using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Xml.Linq;
using System.IO;
using UnityEngine.UI;
using TMPro;

public class MouseVectorize : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public LineRenderer pointRenderer;
    public Vector2 currentPoint;
    public List<Vector2> coordList = new List<Vector2>();
    public List<Vector2> normalizedList = new List<Vector2>();
    public List<Vector2> resampleList = new List<Vector2>();
    public float minDistance = 0.0001f;
    public float phi = 1 / 2 * (-1 + Mathf.Sqrt(5));
    public bool isRecording = false;
    public bool isDraw = false;


    public string fileName = "";
    public TextMeshProUGUI textFile;
    public int nSamples = 32;
    public int boxSize = 5;
    private void Start()
    {
        isRecording = false;
        isDraw = false;
    }

    private void FixedUpdate()
    {
        if (isRecording)
        {
            if (Input.GetMouseButtonDown(0))
            {
                currentPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                coordList.Clear();
                coordList.Add(currentPoint);
            }

            if (Input.GetMouseButton(0))
            {
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                if ((coordList[^1] - mousePos).magnitude > minDistance)
                {
                    coordList.Add(mousePos);
                }

                // visualize
                lineRenderer.positionCount = coordList.Count;
                for (int i = 0; i < coordList.Count; i++)
                {
                    lineRenderer.SetPosition(i, coordList[i] - currentPoint);
                }
            }
            if (Input.GetMouseButtonUp(0))
            {
                resampleList = Resample(coordList, nSamples); // Take sample depends on the situation
                var bbox = boundingBox(resampleList);
                var squarePoints = scaleToSquare(resampleList, boxSize);
                var centralizedPoints = translate2origin(squarePoints);

                // visualize
                pointRenderer.positionCount = centralizedPoints.Count;
                for (int i = 0; i < centralizedPoints.Count; i++)
                {
                    pointRenderer.SetPosition(i, centralizedPoints[i]);
                }
                drawBoundingBox(bbox);
                Template template = new Template(fileName, nSamples, boxSize, centralizedPoints);
                template.saveToXML("Assets/Template");
            }
            return;
        }
        if (isDraw)
        {
            if (Input.GetMouseButtonDown(0))
            {
                currentPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                coordList.Clear();
                coordList.Add(currentPoint);
            }

            if (Input.GetMouseButton(0))
            {
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                if ((coordList[^1] - mousePos).magnitude > minDistance)
                {
                    coordList.Add(mousePos);
                }

                // visualize
                lineRenderer.positionCount = coordList.Count;
                for (int i = 0; i < coordList.Count; i++)
                {
                    lineRenderer.SetPosition(i, coordList[i] - currentPoint);
                }
            }
            if (Input.GetMouseButtonUp(0))
            {
                resampleList = Resample(coordList, nSamples); // Take sample depends on the situation
                var squarePoints = scaleToSquare(resampleList, boxSize);
                var centralizedPoints = translate2origin(squarePoints);
                // visualize
                var bbox = boundingBox(centralizedPoints);
                pointRenderer.positionCount = centralizedPoints.Count;
                for (int i = 0; i < centralizedPoints.Count; i++)
                {
                    pointRenderer.SetPosition(i, centralizedPoints[i]);
                }
                drawBoundingBox(bbox);
                var listOfTemplates = loadAllTemplate("Assets\\Template");
                var result = Recognize(centralizedPoints, listOfTemplates);
                print("Ten la: " + result.Key.XMLpath + "\n" + "So diem du doan la: " + Math.Round(result.Value, 3).ToString());
                if (result.Value < 0.2f)
                {
                    textFile.text = "Khong co hinh nay trong Database";
                }
                else
                {
                    textFile.text = "Ten la: " + result.Key.XMLpath + "\n" + "So diem du doan la: " + Math.Round(result.Value, 3).ToString();
                }
            }
        }
    }

    [SerializeField]
    public void onRecord()
    {
        isRecording = !isRecording;
    }
    [SerializeField]
    public void onDraw()
    {
        isDraw = !isDraw;
    }
    [SerializeField]
    public void getFileName(string s)
    {
        fileName = s;
    }

    //public List<Vector2> normalizePoints(List<Vector2> vector2s)
    //{
    //    var average = new Vector2(vector2s.Average(vector => vector.x), vector2s.Average(vector => vector.y));
    //    var sumOfSquare = new Vector2(vector2s.Select(vector => (vector.x - average.x) * (vector.x - average.x)).Sum(), vector2s.Select(vector => (vector.y - average.y) * (vector.y - average.y)).Sum());
    //    var std = new Vector2(Mathf.Sqrt(sumOfSquare.x / vector2s.Count), Mathf.Sqrt(sumOfSquare.y / vector2s.Count));
    //    var vectorList = new List<Vector2>();
    //    foreach (var vector in vector2s)
    //    {
    //        vectorList.Add(new Vector2((vector.x - average.x) / std.x, (vector.y - average.y) / std.y));
    //    }
    //    return vectorList;
    //}

    // Step 1
    public float pathLength(List<Vector2> vector2s)
    {
        float d = 0;
        for (int i = 1; i < vector2s.Count; i++)
        {
            d += (vector2s[i] - vector2s[i - 1]).magnitude;
        }
        return d;
    }
    public List<Vector2> Resample(List<Vector2> points, int n)
    {
        float I = pathLength(points) / (n - 1);
        float D = 0;
        var newPoints = new List<Vector2>();
        newPoints.Add(points[0]);
        for (int i = 1; i < points.Count; i++)
        {
            float d = (points[i] - points[i - 1]).magnitude;
            if (D + d >= I)
            {
                var q = new Vector2();
                q.x = points[i - 1].x + ((I - D) / d) * (points[i].x - points[i - 1].x);
                q.y = points[i - 1].y + ((I - D) / d) * (points[i].y - points[i - 1].y);
                newPoints.Add(q);
                points[i] = q;
                D = 0;
            }
            else
            {
                D += d;
            }
        }
        return newPoints;
    }


    // Step 2
    public List<Vector2> rotateBy(List<Vector2> points, float phi)
    {
        List<Vector2> newPoints = new List<Vector2>();
        Vector2 c = Centroid(points);
        foreach (Vector2 p in points)
        {
            Vector2 q = new Vector2((p.x - c.x) * Mathf.Cos(phi) - (p.y - c.y) * Mathf.Sin(phi) + c.x, (p.x - c.x) * Mathf.Sin(phi) + (p.y - c.y) * Mathf.Cos(phi) + c.y);
            newPoints.Add(q);
        }
        return newPoints;
    }
    public List<Vector2> rotateToZero(List<Vector2> points)
    {
        Vector2 c = Centroid(points);
        float phi = Mathf.Atan((c.y - points[0].y) / (c.x - points[0].x));
        List<Vector2> newPoints = rotateBy(points, phi);
        return newPoints;
    }

    // Step 3
    public List<Vector2> scaleToSquare(List<Vector2> points, int size)
    {
        var bbox = boundingBox(points);
        var bbWidth = bbox[1].x - bbox[0].x;
        var bbHeight = bbox[1].y - bbox[0].y;
        var newPoints = new List<Vector2>();
        foreach (var point in points)
        {
            newPoints.Add(new Vector2(point.x * (size / bbWidth), point.y * (size / bbHeight)));
        }
        return newPoints;
    }

    public List<Vector2> translate2origin(List<Vector2> points)
    {
        var centroid = Centroid(points);
        var newPoints = new List<Vector2>();
        foreach (var point in points)
        {
            newPoints.Add(new Vector2(point.x - centroid.x, point.y - centroid.y));
        }
        return newPoints;
    }

    // Step 4

    public KeyValuePair<Template, double> Recognize(List<Vector2> points, List<Template> templates)
    {
        float b = float.MaxValue;
        float d;
        Template result = new Template("input", nSamples, boxSize, points);
        foreach (var template in templates)
        {
            d = distanceAtBestAngle(points, template, -45, 45, 2); // Implement theo paper
            if (d < b)
            {
                b = d;
                result = template;
            }
        }
        var score = 1 - b / (0.5 * Mathf.Sqrt(result.rescaleSize * result.rescaleSize * 2));
        return new KeyValuePair<Template, double>(result, score);
    }

    public float distanceAtBestAngle(List<Vector2> points, Template template, float thetaA, float thetaB, float thetaDelta)
    {
        float x1 = phi * thetaA + (1 - phi) * thetaB;
        float f1 = distanceAtAngle(points, template, x1);
        float x2 = (1 - phi) * thetaA + phi * thetaB;
        float f2 = distanceAtAngle(points, template, x2);
        while (Mathf.Abs(thetaA - thetaB) > thetaDelta)
        {
            if (f1 < f2)
            {
                thetaB = x2;
                x2 = x1;
                f2 = f1;
                x1 = phi * thetaA + (1 - phi) * thetaB;
                f1 = distanceAtAngle(points, template, x1);
            }
            else
            {
                thetaA = x1;
                x1 = x2;
                f1 = f2;
                x2 = (1 - phi) * thetaA + phi * thetaB;
                f2 = distanceAtAngle(points, template, x2);
            }
        }
        return Mathf.Min(f1, f2);
    }

    public float distanceAtAngle(List<Vector2> points, Template template, float theta)
    {
        List<Vector2> newPoints = rotateBy(points, theta);
        float d = pathDistance(newPoints, template.templatePoints);
        template.templatePoints.Reverse();
        float dv = pathDistance(newPoints, template.templatePoints); // Make both head and ends
        if (d > dv) return dv;
        return d;
    }

    public float pathDistance(List<Vector2> pointA, List<Vector2> pointB)
    {
        float d = 0f;
        for (int i = 0; i < pointA.Count; i++)
        {
            d += (pointB[i] - pointA[i]).magnitude;
        }
        return d / pointA.Count;
    }

    // Supplementary Functions
    public Vector2 Centroid(List<Vector2> points)
    {
        Vector2 result = new Vector2(0, 0);
        for (int i = 0; i < points.Count; i++)
        {
            result.x += points[i].x;
            result.y += points[i].y;
        }
        result.x = result.x / points.Count;
        result.y = result.y / points.Count;
        return result;
    }

    public List<Vector2> boundingBox(List<Vector2> points)
    {
        var newPoints = new List<Vector2>();
        newPoints.Add(new Vector2(points.Aggregate(points[0].x, (smallest, next) => next.x < smallest ? next.x : smallest, num => num), points.Aggregate(points[0].x, (smallest, next) => next.y < smallest ? next.y : smallest, num => num)));
        newPoints.Add(new Vector2(points.Aggregate(points[0].x, (biggest, next) => next.x > biggest ? next.x : biggest, num => num), points.Aggregate(points[0].x, (biggest, next) => next.y > biggest ? next.y : biggest, num => num)));
        return newPoints;
    }

    public void drawBoundingBox(List<Vector2> bbox)
    {
        var topLeft = bbox[0];
        var bottomRight = bbox[1];
        var topRight = new Vector2(bottomRight.x, topLeft.y);
        var bottomLeft = new Vector2(topLeft.x, bottomRight.y);
        lineRenderer.positionCount = 5;
        lineRenderer.SetPosition(0, topLeft);
        lineRenderer.SetPosition(1, topRight);
        lineRenderer.SetPosition(2, bottomRight);
        lineRenderer.SetPosition(3, bottomLeft);
        lineRenderer.SetPosition(4, topLeft);
    }

    public List<Template> loadAllTemplate(string path)
    {
        List<Template> listTemplates = new List<Template>();
        try
        {
            // Get all Templates within the folder in path
            string[] XFiles = Directory.GetFiles(path, "*.xml");
            foreach (string XFile in XFiles)
            {
                try
                {
                    int nSamples = 0;
                    float rescaleSize = 0;
                    List<Vector2> templatePoints = new List<Vector2>();
                    XDocument doc = XDocument.Load(XFile);
                    string nameFile = Path.GetFileNameWithoutExtension(XFile);
                    if (nameFile == "")
                    {
                        continue;
                    }
                    foreach (XElement gesture in doc.Descendants())
                    {
                        foreach (XElement element in gesture.Elements())
                        {
                            if (element.Name == "Sample")
                            {
                                nSamples = int.Parse(element.Attribute("Samples").Value);
                            }
                            else if (element.Name == "Size")
                            {
                                rescaleSize = float.Parse(element.Attribute("Size").Value);
                            }
                            else if (element.Name == "Templates")
                            {
                                foreach (XElement point in element.Elements())
                                {
                                    Vector2 p = new Vector2(float.Parse(point.Attribute("x").Value), float.Parse(point.Attribute("y").Value));
                                    templatePoints.Add(p);
                                }
                            }
                            // if (element.Name == "Start_End")
                            // {
                            //     foreach (XElement pos in element.Elements())
                            //     {
                            //         if (pos.Name == "Start")
                            //         {
                            //             startPos = new Vector2(float.Parse(pos.Attribute("StartPos_x").Value), float.Parse(pos.Attribute("StartPos_y").Value));
                            //         }
                            //         if (pos.Name == "End")
                            //         {
                            //             endPos = new Vector2(float.Parse(pos.Attribute("EndPos_x").Value), float.Parse(pos.Attribute("EndPos_y").Value));
                            //         }
                            //     }
                            //}
                        }
                    }
                    listTemplates.Add(new Template(nameFile, nSamples, rescaleSize, templatePoints));
                }
                catch (Exception)
                {
                    print("Error");
                }
            }
            return listTemplates;
        }
        catch (Exception)
        {
            print("Error");
            return listTemplates;
        }
    }

    //private void Update()
    //{
    //    if (Input.GetMouseButtonDown(0))
    //    {
    //        currentPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    //        deltaList.Clear();
    //    }

    //    if (Input.GetMouseButton(0))
    //    {
    //        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    //        var delta = mousePos - currentPoint;
    //        deltaList.Add(delta);

    //        // visualize
    //        lineRenderer.positionCount = deltaList.Count;
    //        for (int i = 0; i < deltaList.Count; i++)
    //        {
    //            lineRenderer.SetPosition(i, deltaList[i]);
    //        }
    //    }

    //    if (Input.GetMouseButtonUp(0))
    //    {
    //        Preprocess();
    //    }
    //}

    //public void Preprocess()
    //{
    //    List<Vector2> newDeltaList = new List<Vector2>();
    //    Vector2 lastDelta = Vector2.zero;
    //    float totalCurrentDistance = 0;
    //    for (int i = 0; i < deltaList.Count - 1; i++)
    //    {
    //        Vector2 checking = deltaList[i + 1] - deltaList[i];
    //        lastDelta += checking;
    //        totalCurrentDistance += checking.magnitude;
    //        if (totalCurrentDistance >= minDistance)
    //        {
    //            newDeltaList.Add(lastDelta);
    //            lastDelta = Vector2.zero;
    //            totalCurrentDistance = 0;
    //        }
    //        else
    //        {
    //            lastDelta += checking;
    //        }
    //    }
    //    // visualize
    //    lineRenderer.positionCount = newDeltaList.Count;
    //    for (int i = 0; i < newDeltaList.Count; i++)
    //    {
    //        lineRenderer.SetPosition(i, newDeltaList[i]);
    //    }
    //    print(newDeltaList.Count);
    //}


}
