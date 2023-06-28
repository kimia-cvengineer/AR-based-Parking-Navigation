using System;
using UnityEngine;
using Mapbox.Utils;

public static class GeographicalUtils
{

    public static float GetHeading(Vector2d start, Vector2d end)
    {
        double orgLat = start.x;
        double orgLon = start.y;

        double destLat = end.x;
        double destLon = end.y;


        // Convert latitude and longitude to radians
        orgLat = Math.PI * orgLat / 180;
        destLat = Math.PI * destLat / 180;
        orgLon = Math.PI * orgLon / 180;
        destLon = Math.PI * destLon / 180;

        // Calculate X and Y
        double x = Math.Cos(destLat) * Math.Sin(destLon - orgLon);
        double y = Math.Cos(orgLat) * Math.Sin(destLat) - Math.Sin(orgLat) * Math.Cos(destLat) * Math.Cos(destLon - orgLon);

        // Calculate bearing
        float bearing = (float)Math.Atan2(x, y);

        // Convert bearing to degrees
        bearing = (float)(bearing * 180 / Math.PI);

        // Normalize bearing to between 0 and 360 degrees
        if (bearing < 0)
        {
            bearing += 360;
        }

        return bearing;
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

