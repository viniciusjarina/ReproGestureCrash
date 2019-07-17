using CoreGraphics;
using Foundation;
using System;
using System.Linq;
using System.Reflection;
using UIKit;



namespace ReproGestureCrash
{
	public partial class ViewController : UIViewController
	{
		public ViewController(IntPtr handle) : base(handle)
		{
		}

		object page;
		object renderer;

		const string xaml = @"<ContentPage xmlns=""http://xamarin.com/schemas/2014/forms"" xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"" xmlns:d=""http://xamarin.com/schemas/2014/forms/design"" xmlns:mc=""http://schemas.openxmlformats.org/markup-compatibility/2006"" mc:Ignorable=""d"" x:Class=""masterdetail3.Views.ItemsPage"" Title=""{Binding Title}"" x:Name=""BrowseItemsPage"">
	<StackLayout>
		<ListView x:Name=""ItemsListView"" ItemsSource=""{Binding Items}"" VerticalOptions=""FillAndExpand"" HasUnevenRows=""true"" RefreshCommand=""{Binding LoadItemsCommand}"" IsPullToRefreshEnabled=""true"" IsRefreshing=""{Binding IsBusy, Mode=OneWay}"" CachingStrategy=""RecycleElement"" >
			<d:ListView.ItemsSource>
				<x:Array Type=""{x:Type x:String}"">
					<x:String>Casa Item</x:String>
					<x:String>Correio Item</x:String>
				</x:Array>
			</d:ListView.ItemsSource>
			<ListView.ItemTemplate>
				<DataTemplate>
					<ViewCell>
						<StackLayout Padding=""10"">
							<Label Text=""{Binding Text}"" d:Text=""{Binding .}"" LineBreakMode=""NoWrap"" Style=""{DynamicResource ListItemTextStyle}"" FontSize=""16"" />
							<Label Text=""{Binding Description}"" d:Text=""Item descripton"" LineBreakMode=""NoWrap"" Style=""{DynamicResource ListItemDetailTextStyle}"" FontSize=""13"" />
						</StackLayout>
					</ViewCell>
				</DataTemplate>
			</ListView.ItemTemplate>
		</ListView>
	</StackLayout>
</ContentPage>";


		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			// Perform any additional setup after loading the view, typically from a nib.

			NSTimer timer = NSTimer.CreateScheduledTimer(1 , false, OnTimer);
			//page = new MyContentPage();
			//page = page.LoadFromXaml(xaml);
		}

		void OnTimer(NSTimer timer)
		{
			//Forms.Init();
			var platform = Assembly.Load("Xamarin.Forms.Platform.iOS");

			Type formsType = platform.GetType("Xamarin.Forms.Forms");
			MethodInfo initMethod = formsType.GetMethod("Init", BindingFlags.Static | BindingFlags.Public);
			initMethod.Invoke(null, Array.Empty<object>());
			var library = Assembly.Load("TestLibrary");

			Type contentPageType = library.GetType("masterdetail3.Views.ItemsPage");

			Type platformType = platform.GetType("Xamarin.Forms.Platform.iOS.Platform");

			try
			{
				page = Activator.CreateInstance(contentPageType);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}

			var coreAssembly = Assembly.Load("Xamarin.Forms.Core");

			var xamlAssembly = Assembly.Load("Xamarin.Forms.Xaml");
			Type extension = xamlAssembly.GetType("Xamarin.Forms.Xaml.Extensions");
			MethodInfo methods = extension.GetMethods()[1];
			MethodInfo methodLoadXaml = methods.MakeGenericMethod(contentPageType);
			page = methodLoadXaml.Invoke(null, new object[] { page, xaml });

			object platformObject = Activator.CreateInstance(platformType, true);

			var setPlatform = page.GetType().GetProperty("Platform", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			setPlatform.SetValue(page, platformObject, null);

			MethodInfo createRendererMethod = platformType.GetMethod("CreateRenderer");
			renderer = createRendererMethod.Invoke(null, new object[] { page });

			MethodInfo setRenderer = platformType.GetMethod("SetRenderer");

			setRenderer.Invoke(null, new object[] { page, renderer });

			var nativeView = (UIView)renderer.GetType().GetProperty("NativeView").GetValue(renderer);

			var vc = renderer as UIViewController;
			var window = new UIWindow();
			if (vc != null)
			{
				window.Hidden = false;
				window.RootViewController = vc;
				vc.View.Window.Frame = new CGRect(1000, 1000, vc.View.Frame.Width, vc.View.Frame.Height);
				vc.View.Frame = new CGRect(0, 0, vc.View.Frame.Width, vc.View.Frame.Height);
			}

			var rectangleType = coreAssembly.GetType("Xamarin.Forms.Rectangle");
			var layoutMethod = page.GetType().GetMethods().Single(t =>
			{
				var parameters = t.GetParameters();
				return t.Name == "Layout"
						&& parameters.Length == 1
						&& parameters[0].ParameterType == rectangleType;
			});

			var rectInstance = Activator.CreateInstance(rectangleType);
			rectangleType.GetProperty("X").SetValue(rectInstance, 0d);
			rectangleType.GetProperty("Y").SetValue(rectInstance, 0d);
			rectangleType.GetProperty("Width").SetValue(rectInstance, (double)vc.View.Frame.Width);
			rectangleType.GetProperty("Height").SetValue(rectInstance, (double)vc.View.Frame.Height);
			layoutMethod.Invoke(page, new object[] { rectInstance });

			GC.Collect();
			GC.Collect();
			GC.Collect();
			GC.Collect();

			vc.View.LayoutSubviews();

		}

		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
			// Release any cached data, images, etc that aren't in use.
		}
	}
}