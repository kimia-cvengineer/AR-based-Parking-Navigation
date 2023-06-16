using System;
using UnityEngine;
using Mapbox.Utils;

public static class GeographicalUtils
{

    public static float GetHeading(Vector2d start, Vector2d end)
    {
        float orgLat = (float)start.x;
        float orgLon = (float)start.y;

        float destLat = (float)end.x;
        float destLon = (float)end.y;


        float λ = destLon - orgLon;

        var y = Mathf.Sin(λ) * Mathf.Cos(destLat);
        var x = Mathf.Cos(orgLat) * Mathf.Sin(destLat) -
            Mathf.Sin(orgLat) * Mathf.Cos(destLat) * Mathf.Cos(λ);
        float brng = Mathf.Rad2Deg * Mathf.Atan2(y, x);
        if (brng < 0)
        {
            brng = 360 + brng;
        }
        float compassBrng = Input.compass.trueHeading;
        Debug.Log($"In GetHeading, bearing: {brng}");
        Debug.Log($"In GetHeading, diff compass bearing: { brng - compassBrng}");
        // chatgpt method
        float latitudeA = (float)start.x;  // Latitude of the first coordinate
        float longitudeA = (float)start.y; // Longitude of the first coordinate
        float latitudeB = (float)end.x;  // Latitude of the second coordinate
        float longitudeB = (float)end.y; // Longitude of the second coordinate

        float latitudeARad = Mathf.Deg2Rad * latitudeA;
        float longitudeARad = Mathf.Deg2Rad * longitudeA;
        float latitudeBRad = Mathf.Deg2Rad * latitudeB;
        float longitudeBRad = Mathf.Deg2Rad * longitudeB;
        float deltaLongitudeRad = longitudeBRad - longitudeARad;

        y = Mathf.Sin(deltaLongitudeRad) * Mathf.Cos(latitudeBRad);
        x = Mathf.Cos(latitudeARad) * Mathf.Sin(latitudeBRad) - Mathf.Sin(latitudeARad) * Mathf.Cos(latitudeBRad) * Mathf.Cos(deltaLongitudeRad);

        float headingRad = Mathf.Atan2(y, x);
        float headingDeg = Mathf.Rad2Deg * headingRad;
        //Debug.Log($"In GetHeading, Chat gpt bearing: {headingDeg}");

        return (brng - compassBrng);
    }

    public static Vector2d[] GetPointsBetweenCoordinates(Vector2d start, Vector2d end, int numPoints)
    {
        Debug.Log($"In GetPointsBetweenCoordinates");
        Vector2d[] points = new Vector2d[numPoints];
        
        double startLatRad = ToRadians(start.x);
        double startLonRad = ToRadians(start.y);
        double endLatRad = ToRadians(end.x);
        double endLonRad = ToRadians(end.y);

        double distance = HaversineDistance(startLatRad, startLonRad, endLatRad, endLonRad);
        double bearing = InitialBearing(startLatRad, startLonRad, endLatRad, endLonRad);

        double segmentLength = distance / (numPoints + 1);
        for (int i = 0; i < numPoints; i++)
        {
            double fraction = (i + 1) * segmentLength / distance;
            double intermediateLatRad, intermediateLonRad;
            CalculateIntermediatePoint(startLatRad, startLonRad, bearing, fraction * distance, out intermediateLatRad, out intermediateLonRad);
            points[i] = new Vector2d(ToDegrees(intermediateLatRad), ToDegrees(intermediateLonRad));
        }

        return points;
    }

    private static double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }

    private static double ToDegrees(double radians)
    {
        return radians * 180.0 / Math.PI;
    }

    private static double HaversineDistance(double startLatRad, double startLonRad, double endLatRad, double endLonRad)
    {
        double radius = 6371.0; // Earth's radius in kilometers

        double deltaLat = endLatRad - startLatRad;
        double deltaLon = endLonRad - startLonRad;

        double a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                   Math.Cos(startLatRad) * Math.Cos(endLatRad) *
                   Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        double distance = radius * c;
        return distance;
    }

    private static double InitialBearing(double startLatRad, double startLonRad, double endLatRad, double endLonRad)
    {
        double deltaLon = endLonRad - startLonRad;
        double y = Math.Sin(deltaLon) * Math.Cos(endLatRad);
        double x = Math.Cos(startLatRad) * Math.Sin(endLatRad) -
                   Math.Sin(startLatRad) * Math.Cos(endLatRad) * Math.Cos(deltaLon);

        double bearing = Math.Atan2(y, x);
        return (bearing + 2 * Math.PI) % (2 * Math.PI); // Convert to a positive value between 0 and 2*pi
    }

    private static void CalculateIntermediatePoint(double startLatRad, double startLonRad, double bearing, double distance, out double intermediateLatRad, out double intermediateLonRad)
    {
        double radius = 6371.0; // Earth's radius in kilometers;
        double angularDistance = distance / radius;
        double intermediateLat = Math.Asin(Math.Sin(startLatRad) * Math.Cos(angularDistance) +
                                           Math.Cos(startLatRad) * Math.Sin(angularDistance) * Math.Cos(bearing));
        double intermediateLon = startLonRad + Math.Atan2(Math.Sin(bearing) * Math.Sin(angularDistance) * Math.Cos(startLatRad),
                                                           Math.Cos(angularDistance) - Math.Sin(startLatRad) * Math.Sin(intermediateLat));

        intermediateLatRad = intermediateLat;
        intermediateLonRad = intermediateLon;
    }

    
}

