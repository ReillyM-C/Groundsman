﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Groundsman.Services
{
    public class NavigationService
    {
        public async void NavigateToDetailPage(Feature feature)
        {
            var currentPage = GetCurrentPage();

            await currentPage.Navigation.PushAsync(new FeatureDetailsView(feature));
        }

        public async void NavigateToEditPage(Feature feature)
        {
            var currentPage = GetCurrentPage();

            await currentPage.Navigation.PushAsync(new EditFeatureDetailsView(feature));
        }

        public async void NavigateToNewEditPage(string type)
        {
            var currentPage = GetCurrentPage();

            await currentPage.Navigation.PushAsync(new EditFeatureDetailsView(type));
        }

        public async void PushAddFeaturePage()
        {
            var currentPage = GetCurrentPage();
            if (DeviceInfo.Platform == DevicePlatform.iOS && DeviceInfo.Version < new Version(13, 0))
            {
                await currentPage.Navigation.PushAsync(new AddFeatureView(false));
            } else
            {
                await currentPage.Navigation.PushModalAsync(new AddFeatureView(true));
            }
            
        }

        public async void NavigateBack(bool modal)
        {
            var currentPage = GetCurrentPage();

            if (modal)
            {
                await currentPage.Navigation.PopModalAsync();
            }
            else
            {
                await currentPage.Navigation.PopAsync();
            }
        }

        public Page GetCurrentPage()
        {
            var currentPage = Application.Current.MainPage.Navigation.NavigationStack.LastOrDefault();

            return currentPage;
        }

        public async void InvokeShareSheetAsync()
        {
            await App.FeatureStore.ExportFeatures();
        }
    }
}