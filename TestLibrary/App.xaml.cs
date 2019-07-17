using System;
using Xamarin.Forms;
using masterdetail3.Services;
using masterdetail3.Views;

namespace masterdetail3
{
	public partial class App : Application
	{

		public App()
		{
			InitializeComponent();

			Rectangle rectangle = new Rectangle();
			bool x = false;
			x = typeof(Xamarin.Forms.Xaml.Extensions) != null;
			if (x)
				rectangle.Right = 10;

			DependencyService.Register<MockDataStore>();
			MainPage = new MainPage();
		}

		protected override void OnStart()
		{
			// Handle when your app starts
		}

		protected override void OnSleep()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume()
		{
			// Handle when your app resumes
		}
	}
}
