﻿using System.Threading.Tasks;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Xamarin.Forms;

namespace Groundsman
{
    public partial class HomePage : TabbedPage
    {
        private static HomePage instance;
        public static HomePage Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new HomePage();
                }
                return instance;
            }
        }

        public HomePage()
        {
            InitializeComponent();
            if (CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Location).Result != PermissionStatus.Granted)
            {
                CrossPermissions.Current.RequestPermissionsAsync(Permission.Location);
            }
        }

        /// <summary>
        /// Asynchronously adds DetailFormView to the top of the navigation stack.
        /// </summary>
        /// <param name="type">Data entry type</param>
        public void ShowNewDetailFormPage(string type)
        {
            Navigation.PushModalAsync(new EditFeatureDetailsView());
        }

        public void ShowEditDetailFormPage(Feature entryToEdit)
        {
            Navigation.PushModalAsync(new EditFeatureDetailsView(entryToEdit));
        }

        public async Task ShowExistingDetailFormPage(Feature data)
        {
            await Navigation.PushAsync(new FeatureDetailsView(data));
        }

        public void ShowProfileSettingsPage()
        {
            Navigation.PushModalAsync(new ProfileSettingsView());
        }

        /// <summary>
        /// Displays a pop-up user interface to navigate to different data entry types
        /// </summary>
        /// <returns></returns>
        public async Task ShowDetailFormOptions()
        {
            await Navigation.PushModalAsync(new EditFeatureDetailsView());
        }
    }
}
