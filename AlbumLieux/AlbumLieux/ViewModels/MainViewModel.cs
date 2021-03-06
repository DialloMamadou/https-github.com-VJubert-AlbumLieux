﻿using AlbumLieux.Models;
using AlbumLieux.Pages;
using AlbumLieux.Services;
using Storm.Mvvm;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Plugin.Geolocator.Abstractions;
using System.Linq;
using AlbumLieux.Exceptions;

namespace AlbumLieux.ViewModels
{
	public class MainViewModel : ViewModelBase
	{
		private readonly Lazy<IPlacesDataServices> _placesService = new Lazy<IPlacesDataServices>(() => DependencyService.Get<IPlacesDataServices>());
		private readonly Lazy<ITokenService> _tokenService = new Lazy<ITokenService>(() => DependencyService.Get<ITokenService>());
		private readonly Lazy<IGeolocationService> _geoService = new Lazy<IGeolocationService>(() => DependencyService.Get<IGeolocationService>());
		private readonly Lazy<IDialogService> _dialogService = new Lazy<IDialogService>(() => DependencyService.Resolve<IDialogService>());

		public ICommand RefreshCommand { get; }
		public ICommand ProfileCommand { get; }
		public ICommand AddCommand { get; }

		private bool _isBusy;

		public bool IsBusy
		{
			get => _isBusy;
			set => SetProperty(ref _isBusy, value);
		}

		private Places _selectedPlace;

		public Places SelectedPlace
		{
			get => _selectedPlace;
			set
			{
				if (SetProperty(ref _selectedPlace, value) && value != null)
				{
					OnItemClicked(value);
				}
			}
		}

		private List<Places> _spotList;

		public List<Places> SpotList
		{
			get => _spotList;
			set => SetProperty(ref _spotList, value);
		}

		private bool _isConnected;
		public bool IsConnected
		{
			get => _isConnected;
			set => SetProperty(ref _isConnected, value);
		}

		public MainViewModel()
		{
			RefreshCommand = new Command(RefreshAction);
			ProfileCommand = new Command(ProfileAction);
			AddCommand = new Command(AddAction);
		}

		public override async Task OnResume()
		{
			try
			{
				await base.OnResume();

				IsBusy = true;

				await RefreshList();
				IsConnected = _tokenService.Value.HasToken();

				SelectedPlace = null;
			}
			finally
			{
				IsBusy = false;
			}
		}

		private async void AddAction()
		{
			await NavigationService.PushAsync<AddPlacePage>();
		}

		private async void ProfileAction()
		{
			if (IsConnected)
			{
				await NavigationService.PushAsync<ProfilePage>();
			}
			else
			{
				await NavigationService.PushAsync<LoginPage>();
			}
		}

		private async Task RefreshList(bool force = false)
		{
			try
			{
				var list = await _placesService.Value.ListPlaces(force);
				var position = await _geoService.Value.GetMyPosition();

				if (position != null)
				{
					SpotList = list.Select(x =>
					{
						x.DistanceToMe = position.CalculateDistance(new Position(x.Latitude, x.Longitude), GeolocatorUtils.DistanceUnits.Kilometers);
						return x;
					}).OrderBy(x => x.DistanceToMe).ToList();
				}
				else
				{
					SpotList = list;
				}
			}
			catch (NotSupportedException)
			{
				await _dialogService.Value.ShowAlertDialog("Impossible", "L'action demandé est impossible à réaliser sur le device", "Ok");
			}
			catch (MissingPermissionException permEx)
			{
				await _dialogService.Value.ShowAlertDialog("Permission manquante", $"La permission {permEx.PermissionName} est manquante pour executer l'action demandée", "Ok");
			}
			catch (ApiException apiEx)
			{
				await _dialogService.Value.ShowAlertDialog("Erreur", apiEx.ErrorMessage, "Ok");
			}
		}

		private async void RefreshAction()
		{
			IsBusy = true;
			await RefreshList(true);
			IsBusy = false;
		}

		private async Task OnItemClicked(Places obj)
		{
			await NavigationService.PushAsync<DetailTabbedPage>(new Dictionary<string, object>
			{
				["id"] = obj.Id
			});
		}
	}
}