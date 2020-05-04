﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Maps;

namespace Groundsman
{
    /// <summary>
    /// ViewModel for maps page
    /// </summary>
    public class MapViewModel : ViewModelBase
    {
        private CancellationTokenSource cts;

        public MapViewModel()
        {
            Map = new CustomMap();
            CenterMapOnUser();
            Map.MapClicked += OnMapClicked;
        }

        public CustomMap Map { get; private set; }

        // Only center map on user if location permissions are granted
        private async void CenterMapOnUser()
        {
            var status = await Services.CheckAndRequestPermissionAsync(new Permissions.LocationWhenInUse());
            if (status != PermissionStatus.Granted)
            {
                Map.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(-27.47004901089882, 153.021072), Distance.FromMiles(1.0)));
                return;
            }
            else
            {
                Point location = await Services.GetGeoLocation();
                Map.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(location.Latitude, location.Longitude), Distance.FromMiles(1.0)));
            }
        }

        public void CleanFeatures()
        {
            Map.MapElements.Clear();
            Map.Pins.Clear();
        }

        public void DrawFeatures()
        {
            // Using CurrentFeature to draw the geodata on the map
            ObservableCollection<Feature> Features = App.FeatureStore.CurrentFeatures;
            Features.ForEach((Feature feature) =>
            {
                var points = feature.properties.xamarincoordinates;
                if (feature.geometry.type.Equals("Point") && Preferences.Get("ShowPointsOnMap", true))
                {
                    Pin pin = new Pin
                    {
                        Label = feature.properties.name,
                        Address = string.Format("{0}, {1}, {2}", points[0].Latitude, points[0].Longitude, points[0].Altitude),
                        Type = PinType.Place,
                        Position = new Position(points[0].Latitude, points[0].Longitude)
                    };
                    Map.Pins.Add(pin);
                }
                else if ((feature.geometry.type.Equals("Line") || feature.geometry.type.Equals("LineString")) && Preferences.Get("ShowLinesOnMap", true))
                {
                    Debug.WriteLine("Line");
                    Polyline polyline = new Polyline
                    {
                        StrokeColor = Color.OrangeRed,
                        StrokeWidth = 5,
                    };
                    points.ForEach((Point point) =>
                    {
                        polyline.Geopath.Add(new Position(point.Latitude, point.Longitude));
                    });
                    Map.MapElements.Add(polyline);
                }
                else if (feature.geometry.type.Equals("Polygon") && Preferences.Get("ShowPolygonsOnMap", true))
                {
                    Polygon polygon = new Polygon
                    {
                        StrokeWidth = 4,
                        StrokeColor = Color.OrangeRed,
                        FillColor = Color.FromHex("#85cb5748"),
                    };
                    points.ForEach((Point point) =>
                    {
                        polygon.Geopath.Add(new Position(point.Latitude, point.Longitude));
                    });
                    Map.MapElements.Add(polygon);
                }
            });
        }

        public async void RefreshMap()
        {
            CleanFeatures();
            DrawFeatures();

            if (Preferences.Get("ShowLogPathOnMap", true))
            {
                //Setup log
                cts = new CancellationTokenSource();
                _ = MapLogUpdaterAsync(new TimeSpan(0, 0, 1), cts.Token);
            }
            //SetShowingUser
            var status = await Services.CheckAndRequestPermissionAsync(new Permissions.LocationWhenInUse());
            if (status == PermissionStatus.Granted)
            {
                Map.IsShowingUser = true;
            }
            else
            {
                Map.IsShowingUser = false;
            }
        }

        private async Task MapLogUpdaterAsync(TimeSpan interval, CancellationToken ct)
        {
            while (true)
            {
                await Task.Delay(interval, ct);
                List<Point> logFile = App.LogStore.GetLogFileObject();
                Polyline logPolyline = new Polyline
                {
                    StrokeColor = Color.DarkOrange,
                    StrokeWidth = 3,
                };
                logFile.ForEach((Point point) =>
                {
                    logPolyline.Geopath.Add(new Position(point.Latitude, point.Longitude));
                });
                Map.MapElements.Add(logPolyline);
            }
        }

        public void CleanupLog()
        {
            if (cts != null)
            {
                cts.Cancel();
            }
        }

        void OnMapClicked(object sender, MapClickedEventArgs e)
        {
            ObservableCollection<Feature> Features = App.FeatureStore.CurrentFeatures;
            Features.ForEach(async (Feature feature) =>
            {
                bool ItemHit = false;
                Point[] points = feature.properties.xamarincoordinates.ToArray();
                if (feature.geometry.type.Equals("Polygon"))
                {
                    ItemHit |= IsPointInPolygon(new Point(e.Position.Latitude, e.Position.Longitude, 0), points);
                }
                else if (feature.geometry.type.Equals("Line") || feature.geometry.type.Equals("LineString"))
                {
                    ItemHit |= IsPointOnLine(new Point(e.Position.Latitude, e.Position.Longitude, 0), points);
                }

                if (ItemHit)
                {
                    string pointString = "";
                    for (int i = 0; i < points.Length; i++)
                    {
                        pointString += string.Format("{0}, {1}, {2} \n", points[i].Latitude, points[i].Longitude, points[i].Altitude);
                    }

                    string result = await HomePage.Instance.DisplayActionSheet(feature.properties.name, "Dismiss", "Delete", "View", "Edit");

                    switch (result)
                    {
                        case "Delete":
                            bool yesResponse = await HomePage.Instance.DisplayAlert("Delete Feature", "Are you sure you want to delete this feature?", "Yes", "No");
                            if (yesResponse)
                            {
                                App.FeatureStore.DeleteFeatureAsync(feature);
                            }
                            break;
                        case "View":
                            await HomePage.Instance.ShowDetailFormPage(feature);
                            break;
                        case "Edit":
                            await HomePage.Instance.ShowEditDetailFormPage(feature);
                            break;
                        default:
                            break;
                    }
                }
            });
        }

        public bool IsPointInPolygon(Point p, Point[] polygon)
        {
            double minX = polygon[0].Longitude;
            double maxX = polygon[0].Longitude;
            double minY = polygon[0].Latitude;
            double maxY = polygon[0].Latitude;
            for (int i = 1; i < polygon.Length; i++)
            {
                Point q = polygon[i];
                minX = Math.Min(q.Longitude, minX);
                maxX = Math.Max(q.Longitude, maxX);
                minY = Math.Min(q.Latitude, minY);
                maxY = Math.Max(q.Latitude, maxY);
            }

            if (p.Longitude < minX || p.Longitude > maxX || p.Latitude < minY || p.Latitude > maxY)
            {
                return false;
            }

            // http://www.ecse.rpi.edu/Homepages/wrf/Research/Short_Notes/pnpoly.html
            bool inside = false;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                if ((polygon[i].Latitude > p.Latitude) != (polygon[j].Latitude > p.Latitude) &&
                     p.Longitude < (polygon[j].Longitude - polygon[i].Longitude) * (p.Latitude - polygon[i].Latitude) / (polygon[j].Latitude - polygon[i].Latitude) + polygon[i].Longitude)
                {
                    inside = !inside;
                }
            }
            return inside;
        }

        //currently only works on line vertices
        public bool IsPointOnLine(Point p, Point[] polyline)
        {
            for (int i = 0; i < polyline.Length; i++)
            {
                Point q = polyline[i];
                if (Math.Abs(p.Latitude - q.Latitude) <= .0003 && Math.Abs(p.Longitude - q.Longitude) <= .0003)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
