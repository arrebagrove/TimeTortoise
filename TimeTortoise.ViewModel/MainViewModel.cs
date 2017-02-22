﻿using System.Collections.ObjectModel;
using TimeTortoise.DAL;
using TimeTortoise.Model;

namespace TimeTortoise.ViewModel
{
	public class MainViewModel : NotificationBase
	{
		private readonly IRepository _repository;

		public MainViewModel() : this(new Repository(new SqliteContext()))
		{
		}

		public MainViewModel(IRepository repository)
		{
			_repository = repository;
			LoadActivities();
		}

		public ObservableCollection<ActivityViewModel> Activities { get; private set; } = new ObservableCollection<ActivityViewModel>();

		private int _selectedIndex = -1;
		public int SelectedIndex
		{
			get { return _selectedIndex; }
			set
			{
				SetProperty(ref _selectedIndex, value);
				if (Activities.Count == 0) Activities.Add(new ActivityViewModel(new Activity()));
				if (_selectedIndex <= 0) _selectedIndex = 0;
				SelectedActivity = Activities[_selectedIndex];
			}
		}

		private ActivityViewModel _selectedActivity = new ActivityViewModel(new Activity());
		public ActivityViewModel SelectedActivity
		{
			get { return _selectedActivity; }
			private set { SetProperty(ref _selectedActivity, value); }
		}

		public void LoadActivities()
		{
			var activities = _repository.LoadActivities();
			Activities = new ObservableCollection<ActivityViewModel>();
			foreach (var activity in activities) Activities.Add(new ActivityViewModel(activity));
		}

		public void Save()
		{
			_repository.SaveActivity(Activities[_selectedIndex]);
		}

		public void Add()
		{
			Activities.Add(new ActivityViewModel(new Activity()));
			SelectedIndex = Activities.Count - 1;
		}
	}
}