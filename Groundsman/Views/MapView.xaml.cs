﻿using Groundsman.ViewModels;
using Xamarin.Forms;

namespace Groundsman.Views
{
    public partial class MapView : ContentPage
    {
        private readonly MapViewModel viewModel;
        public MapView()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            InitializeComponent();
            BindingContext = viewModel = new MapViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            viewModel.RefreshMap();
        }
    }
}