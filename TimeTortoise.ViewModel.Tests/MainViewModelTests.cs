﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

using Xunit;
using Moq;

using TimeTortoise.Client;
using TimeTortoise.Model;
using TimeTortoise.TestHelper;

namespace TimeTortoise.ViewModel.Tests
{
	public class MainViewModelTests
	{
		[Fact]
		public void MainViewModel_WhenActivitiesAreLoaded_ContainsActivities()
		{
			// Arrange
			var mvm = Helper.GetMainViewModel();

			// Act
			mvm.LoadActivities();

			// Assert
			Assert.Equal(3, mvm.Activities.Count);
			Assert.Equal("Activity1", mvm.Activities[0].Name);
			Assert.Equal("Activity2", mvm.Activities[1].Name);
			Assert.Equal("3", mvm.Activities[1].GetTimeSegment(1).StartTime.Substring(0, 1));
			Assert.Equal("Activity3", mvm.Activities[2].Name);
		}

		[Fact]
		public void MainViewModel_WhenActivitiesAndTimeSegmentsAreLoaded_ActivityContainsTimeSegments()
		{
			// Arrange
			var mvm = Helper.GetMainViewModel();

			// Act
			mvm.LoadActivities();
			mvm.SelectedActivityIndex = 1;
			mvm.LoadTimeSegments();

			// Assert
			Assert.Equal(4, mvm.SelectedActivity.NumObservableTimeSegments);
		}

		[Fact]
		public void TimeSegmentList_WhenTimingIsStarted_ContainsStartedTimeSegment()
		{
			// Arrange
			var mockRepository = Helper.GetMockRepository();
			var mvm = Helper.GetMainViewModel(mockRepository.Object);

			// Act
			mvm.LoadActivities();
			mvm.SelectedActivityIndex = 1;
			mvm.LoadTimeSegments();
			mvm.StartStop();
			var ots = mvm.SelectedActivity.ObservableTimeSegments;
			var startedTimeSegment = ots[ots.Count - 1];
			mockRepository.Setup(x => x.LoadTimeSegments(1, It.IsAny<IDateTime>(), It.IsAny<IDateTime>())).Returns(
				new List<TimeSegment>
				{
					new TimeSegment
					{
						ActivityId =  startedTimeSegment.TimeSegment.ActivityId,
						StartTime = startedTimeSegment.TimeSegment.StartTime,
						EndTime = startedTimeSegment.TimeSegment.EndTime
					}
				});
			mvm.LoadTimeSegments();
			ots = mvm.SelectedActivity.ObservableTimeSegments;
			startedTimeSegment = ots[ots.Count - 1];

			// Assert
			Assert.Equal(startedTimeSegment, mvm.StartedTimeSegment);

			// Act
			mvm.StartStop();
			mvm.LoadTimeSegments();

			// Assert
			Assert.NotEqual(startedTimeSegment, mvm.StartedTimeSegment);
		}

		[Fact]
		public void SelectedActivity_WhenNoActivityIsSelected_IsNull()
		{
			// Arrange
			var mvm = Helper.GetMainViewModel();

			// Act
			var avm = mvm.SelectedActivity;

			// Assert
			Assert.Null(avm);
		}

		[Fact]
		public void SelectedTimeSegment_WhenNoTimeSegmentIsSelected_IsNull()
		{
			// Arrange
			var mvm = Helper.GetMainViewModel();

			// Act
			var tvm = mvm.SelectedTimeSegment;

			// Assert
			Assert.Null(tvm);
		}

		[Fact]
		public void SetSelectedTimeSegment_WhenSelectedTimeSegmentIndexIsInvalid_SetsSelectedTimeSegmentToNull()
		{
			// Arrange
			var mvm = Helper.GetMainViewModel(1);
			mvm.SelectedTimeSegmentIndex = 0;

			var tvm = mvm.SelectedTimeSegment;
			Assert.NotNull(tvm);

			// Act
			mvm.SelectedTimeSegmentIndex = -1;
			tvm = mvm.SelectedTimeSegment;

			// Assert
			Assert.Null(tvm);
		}

		[Fact]
		public void SelectedTimeSegment_WhenNoActivityIsSelected_IsNull()
		{
			// Arrange
			var mvm = Helper.GetMainViewModel();
			mvm.SelectedTimeSegmentIndex = 1;

			// Act
			var tvm = mvm.SelectedTimeSegment;

			// Assert
			Assert.Null(tvm);
		}

		[Fact]
		public void SelectedTimeSegmentStartAndEndTimes_WhenNoTimeSegmentIsSelected_AreEmpty()
		{
			// Arrange
			var mvm = Helper.GetMainViewModel();

			// Assert
			Assert.Equal(string.Empty, mvm.SelectedTimeSegmentStartTime);
			Assert.Equal(string.Empty, mvm.SelectedTimeSegmentEndTime);
		}

		[Fact]
		public void SelectedActivity_WhenNameIsUpdated_IsUpdatedInActivityList()
		{
			// Arrange
			var mvm = Helper.GetMainViewModel();

			// Act
			mvm.AddActivity();
			mvm.SelectedActivity.Name = "TestName1";

			// Assert
			Assert.Equal("TestName1", mvm.Activities[mvm.Activities.Count - 1].Name);
		}

		[Fact]
		public void SelectedTimeSegment_WhenEndTimeIsUpdated_IsUpdatedInTimeSegmentList()
		{
			// Arrange
			var endTime = "3/1/2017 1:00:00 PM";
			var mvm =
				new MainViewModel(Helper.GetMockRepositoryObject(), new SystemDateTime(), new ValidationMessageViewModel(), Helper.GetMockSignalRClientObject(), Helper.GetMockSettingsUtility())
				{
					SelectedActivityIndex = 1,
					SelectedTimeSegmentIndex = 0,
					SelectedTimeSegment = { EndTime = endTime }
				};

			// Assert
			Assert.Equal(endTime, mvm.SelectedActivity.GetTimeSegment(0).EndTime);
			Assert.Equal(endTime, mvm.SelectedTimeSegmentEndTime);
		}

		[Fact]
		public void SelectedActivityIndex_WhenNewActivityIsAdded_PointsToNewActivity()
		{
			// Arrange
			var mvm = Helper.GetMainViewModel();

			// Act
			mvm.AddActivity();
			mvm.AddActivity();

			// Assert
			Assert.Equal(4, mvm.SelectedActivityIndex);
		}

		[Fact]
		public void SelectedTimeSegment_WhenNewTimeSegmentIsAdded_PointsToNewTimeSegment()
		{
			// Arrange
			var mvm = Helper.GetMainViewModel();
			mvm.AddActivity();

			// Act
			mvm.AddTimeSegment();
			mvm.AddTimeSegment();
			var ts = mvm.SelectedActivity.GetTimeSegment(1);

			// Assert
			Assert.Equal(1, mvm.SelectedTimeSegmentIndex);
			Assert.Equal(ts, mvm.SelectedTimeSegment);
		}

		[Fact]
		public void IsSaveEnabled_WhenNewActivityIsAdded_SwitchesToTrue()
		{
			// Arrange
			var mvm = Helper.GetMainViewModel();

			// Act
			Assert.False(mvm.IsSaveEnabled);
			mvm.AddActivity();

			// Assert
			Assert.True(mvm.IsSaveEnabled);
		}

		[Fact]
		public void IsTimeSegmentDeleteEnabled_WhenNewTimeSegmentIsAdded_SwitchesToTrue()
		{
			// Arrange
			var mvm = Helper.GetMainViewModel(0);
			Assert.False(mvm.IsTimeSegmentDeleteEnabled);

			// Act
			mvm.AddTimeSegment();

			// Assert
			Assert.True(mvm.IsTimeSegmentDeleteEnabled);
		}

		[Fact]
		public void IsTimeSegmentAddEnabled_WhenActivityIsSelected_SwitchesToTrue()
		{
			// Arrange
			var mvm = Helper.GetMainViewModel();

			// Act
			Assert.False(mvm.IsTimeSegmentAddEnabled);
			mvm.SelectedActivityIndex = 0;

			// Assert
			Assert.True(mvm.IsTimeSegmentAddEnabled);
		}

		[Fact]
		public void SaveAndDeleteButtons_WhenSelectedTimeSegmentIndexIsInRange_AreEnabled()
		{
			// Arrange
			var mvm = Helper.GetMainViewModel(1);
			mvm.SelectedTimeSegmentIndex = 3;

			// Assert
			Assert.True(mvm.IsSaveEnabled);
			Assert.True(mvm.IsTimeSegmentDeleteEnabled);
		}

		[Fact]
		public void IsSaveEnabled_WhenSelectedActivityIndexIsOutOfRangeLow1_IsFalse()
		{
			// Arrange
			var mvm = Helper.GetMainViewModel();

			// Assert
			Assert.False(mvm.IsSaveEnabled);
		}

		[Fact]
		public void SaveAndDeleteButtons_WhenSelectedTimeSegmentIndexIsOutOfRangeLow1_AreDisabled()
		{
			// Arrange
			var mvm = Helper.GetMainViewModel();

			// Assert
			Assert.False(mvm.IsSaveEnabled);
		}

		[Fact]
		public void IsSaveEnabled_WhenSelectedActivityIndexIsOutOfRangeLow2_IsFalse()
		{
			// Arrange
			var mvm = Helper.GetMainViewModel(-1);

			// Assert
			Assert.False(mvm.IsSaveEnabled);
		}

		[Fact]
		public void IsSaveEnabled_WhenSelectedTimeSegmentIndexIsOutOfRangeLow2_IsFalse()
		{
			// Arrange
			var mvm = Helper.GetMainViewModel();
			mvm.SelectedTimeSegmentIndex = -1;

			// Assert
			Assert.False(mvm.IsSaveEnabled);
		}

		[Fact]
		public void IsSaveEnabled_WhenSelectedActivityIndexIsOutOfRangeHigh_IsFalse()
		{
			// Arrange
			var mvm = Helper.GetMainViewModel(99);

			// Assert
			Assert.False(mvm.IsSaveEnabled);
		}

		[Fact]
		public void ActivityList_WhenActivityIsDeleted_IsUpdated()
		{
			var mvm = Helper.GetMainViewModel();
			mvm.LoadActivities();

			// Act
			mvm.SelectedActivityIndex = 0;
			mvm.DeleteActivity();

			// Assert
			Assert.Equal(2, mvm.Activities.Count);
		}

		[Fact]
		public void Activity_WhenAddedThenDeletedThenAdded_IsValid()
		{
			var mvm = Helper.GetMainViewModel();
			mvm.LoadActivities();

			// Act
			mvm.AddActivity();
			mvm.DeleteActivity();
			mvm.AddActivity();

			// Assert
			Assert.Equal(4, mvm.Activities.Count);
		}

		[Fact]
		public void TimingState_WhenStartedActivityIsDeleted_IsCorrect()
		{
			var mvm = Helper.GetMainViewModel();
			mvm.LoadActivities();

			// Act
			mvm.AddActivity();
			mvm.StartStop();
			mvm.DeleteActivity();

			// Assert
			Assert.Null(mvm.StartedActivity);
			Assert.Equal("Start", mvm.StartStopText);
		}

		[Fact]
		public void TimingState_WhenStartedTimeSegmentIsDeleted_IsCorrect()
		{
			var mvm = Helper.GetMainViewModel();
			mvm.LoadActivities();

			// Act
			mvm.AddActivity();
			mvm.StartStop();
			mvm.SelectedTimeSegmentIndex = 0;
			mvm.DeleteTimeSegment();

			// Assert
			Assert.Null(mvm.StartedActivity);
			Assert.Equal("Start", mvm.StartStopText);
		}

		[Fact]
		public void TimeSegmentList_WhenTimeSegmentIsDeleted_IsUpdated()
		{
			var mvm = Helper.GetMainViewModel();
			mvm.LoadActivities();

			// Act
			mvm.SelectedActivityIndex = 1;
			mvm.SelectedTimeSegmentIndex = 1;
			mvm.DeleteTimeSegment();

			// Assert
			Assert.Equal(3, mvm.SelectedActivity.NumObservableTimeSegments);
		}

		[Fact]
		public void TimeSegmentList_AfterStartAddDelete_IsInCorrectState()
		{
			var mvm = Helper.GetMainViewModel();

			// Act
			mvm.AddActivity();
			mvm.StartStop();
			mvm.AddTimeSegment();
			mvm.DeleteTimeSegment();

			// Assert
			Assert.Equal(1, mvm.SelectedActivity.NumObservableTimeSegments);
		}

		[Fact]
		public void ActivityName_WhenDeleted_FiresCorrectEvents()
		{
			// Arrange
			var mvm = Helper.GetMainViewModel();
			// http://stackoverflow.com/a/249042/4803
			var receivedEvents = new List<string>();
			mvm.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
			{
				receivedEvents.Add(e.PropertyName);
			};

			// Act
			mvm.AddActivity();
			mvm.SelectedActivity.Name = "TestName1";

			// Assert
			var n = 0;
			Assert.Equal("SelectedActivityIndex", receivedEvents[n++]);
			Assert.Equal("IsSaveEnabled", receivedEvents[n++]);
			Assert.Equal("IsTimeSegmentAddEnabled", receivedEvents[n++]);
			Assert.Equal("SelectedActivity", receivedEvents[n++]);
			Assert.True(n > 0);
		}

		[Fact]
		public void StartStopButton_WhenPressed_TogglesButtonText()
		{
			// Arrange
			var mvm = Helper.GetMainViewModel(0);

			// Act
			mvm.StartStop();

			// Assert
			Assert.Equal("Stop", mvm.StartStopText);
			mvm.StartStop();
			Assert.Equal("Start", mvm.StartStopText);
		}

		[Fact]
		public void TimeSegment_WhenStartAndEndAreOneHourApart_HasElapsedTimeOfOneHour()
		{
			// Arrange
			var startTime = new Mock<IDateTime>();
			startTime.Setup(x => x.Now).Returns(new DateTime(2017, 3, 1, 10, 0, 0));
			var endTime = new Mock<IDateTime>();
			endTime.Setup(x => x.Now).Returns(new DateTime(2017, 3, 1, 11, 0, 0));

			// Act
			var elapsedTime = endTime.Object.Now - startTime.Object.Now;

			// Assert
			Assert.Equal(1, elapsedTime.Hours);
		}

		[Fact]
		public void TimeSegments_WhenTimingStarts_HasOneEntry()
		{
			// Arrange
			var mvm = Helper.GetMainViewModel(0);

			// Act
			mvm.StartStop();

			// Assert
			Assert.Equal(1, mvm.SelectedActivity.NumObservableTimeSegments);
		}

		[Fact]
		public void TimeSegment_WhenTimingStarts_HasCorrectStartAndEndTimes()
		{
			// Arrange
			var mockTime = new Mock<IDateTime>();
			var startTime = new DateTime(2017, 3, 1, 10, 0, 0);
			mockTime.Setup(x => x.Now).Returns(startTime);

			var mvm = Helper.GetMainViewModel(0, mockTime.Object, Helper.GetMockSignalRClientObject());

			// Act
			mvm.StartStop();
			mvm.SelectedTimeSegmentIndex = 0;

			// Assert
			Assert.Equal(startTime.ToString(CultureInfo.CurrentUICulture), mvm.SelectedTimeSegment.StartTime);
			Assert.Equal(startTime.ToString(CultureInfo.CurrentUICulture), mvm.SelectedTimeSegmentStartTime);
			Assert.Equal(startTime.ToString(CultureInfo.CurrentUICulture), mvm.SelectedTimeSegment.EndTime);
			Assert.Equal(startTime.ToString(CultureInfo.CurrentUICulture), mvm.SelectedTimeSegmentEndTime);
		}

		[Fact]
		public void TimeSegment_WhenTimingEnds_HasCorrectStartAndEndTimes()
		{
			// Arrange
			var mockTime = new Mock<IDateTime>();
			var startTime = new DateTime(2017, 3, 1, 10, 0, 0);
			mockTime.Setup(x => x.Now).Returns(startTime);

			var mvm = Helper.GetMainViewModel(0, mockTime.Object, Helper.GetMockSignalRClientObject());

			// Act
			mvm.StartStop();
			var endTime = new DateTime(2017, 3, 1, 11, 0, 0);
			mockTime.Setup(x => x.Now).Returns(endTime);
			mvm.StartStop();
			mvm.SelectedTimeSegmentIndex = 0;

			// Assert
			Assert.Equal(startTime.ToString(CultureInfo.CurrentUICulture), mvm.SelectedTimeSegment.StartTime);
			Assert.Equal(startTime.ToString(CultureInfo.CurrentUICulture), mvm.SelectedTimeSegmentStartTime);
			Assert.Equal(endTime.ToString(CultureInfo.CurrentUICulture), mvm.SelectedTimeSegment.EndTime);
			Assert.Equal(endTime.ToString(CultureInfo.CurrentUICulture), mvm.SelectedTimeSegmentEndTime);
		}

		[Fact]
		public void TimeSegment_WhenAlternateStartAndEndAccessorsAreSet_HasCorrectStartAndEndTimes()
		{
			// Arrange
			const string startTime = "3/1/2017 10:00:00 AM";
			const string endTime = "3/1/2017 11:00:00 AM";

			var mvm = Helper.GetMainViewModel(0);

			// Act
			mvm.AddTimeSegment();
			mvm.SelectedTimeSegmentStartTime = startTime;
			mvm.SelectedTimeSegmentEndTime = endTime;

			// Assert
			Assert.Equal(startTime, mvm.SelectedTimeSegment.StartTime);
			Assert.Equal(endTime, mvm.SelectedTimeSegment.EndTime);
		}

		[Fact]
		public void TimeSegment_WhenTimingEnds_IsSaved()
		{
			// Arrange
			var mockRepository = Helper.GetMockRepository();
			var mockTime = new Mock<IDateTime>();
			var startTime = new DateTime(2017, 3, 1, 10, 0, 0);
			mockTime.Setup(x => x.Now).Returns(startTime);

			var mvm = new MainViewModel(mockRepository.Object, mockTime.Object, new ValidationMessageViewModel(),
				Helper.GetMockSignalRClientObject(), Helper.GetMockSettingsUtility())
			{
				SelectedActivityIndex = 0
			};

			// Act
			mvm.StartStop();
			var endTime = new DateTime(2017, 3, 1, 11, 0, 0);
			mockTime.Setup(x => x.Now).Returns(endTime);
			mvm.StartStop();

			// Assert
			mockRepository.Verify(x => x.SaveChanges());
		}

		[Fact]
		public void TimeSegmentList_WhenTimerStops_UpdatesUI()
		{
			// Arrange
			var mvm = Helper.GetMainViewModel();
			var receivedEvents = new List<string>();

			// Act
			mvm.AddActivity();
			mvm.SelectedActivityIndex = 0;
			mvm.SelectedActivity.ObservableTimeSegments.CollectionChanged += delegate (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
			{
				receivedEvents.Add(e.Action.ToString());
			};
			mvm.StartStop();
			mvm.StartStop();

			// Assert
			Assert.Equal("Add", receivedEvents[0]);
			Assert.Equal("Replace", receivedEvents[1]);
		}

		[Fact]
		public void ValidationMessage_WhenStartTimeIsValid_IsEmpty()
		{
			// Arrange
			var mvm = Helper.GetMainViewModel(out ValidationMessageViewModel vmvm);
			mvm.LoadActivities();

			// Act
			mvm.SelectedActivityIndex = 1;
			mvm.SelectedTimeSegmentIndex = 0;
			mvm.SelectedTimeSegment.StartTime = "3/1/2017 1:00 PM";

			// Assert
			Assert.Equal(string.Empty, vmvm.ValidationMessages);
		}

		[Fact]
		public void ValidationMessage_WhenStartTimeIsInvalid_HasCorrectMessage()
		{
			// Arrange
			var mvm = Helper.GetMainViewModel(out ValidationMessageViewModel vmvm);
			mvm.LoadActivities();

			// Act
			mvm.AddActivity();
			mvm.AddTimeSegment();
			mvm.SelectedTimeSegment.StartTime = "2/31/2017 1:00 PM";

			// Assert
			Assert.Equal("Please enter a valid start date and time.\r\n", vmvm.ValidationMessages);
		}

		[Fact]
		public void ValidationMessage_WhenEndTimeIsInvalid_HasCorrectMessage()
		{
			// Arrange
			var mvm = Helper.GetMainViewModel(out ValidationMessageViewModel vmvm);
			mvm.LoadActivities();

			// Act
			mvm.AddActivity();
			mvm.AddTimeSegment();
			mvm.SelectedTimeSegment.EndTime = "2/31/2017 1:00 PM";

			// Assert
			Assert.Equal("Please enter a valid end date and time.\r\n", vmvm.ValidationMessages);
		}

		[Fact]
		public void SelectedTimeSegmentStartTime_WhenNoTimeSegmentIsSelected_DoesntThrow()
		{
			// Arrange/Act
			var mvm = Helper.GetMainViewModel(string.Empty);

			// Assert
			Assert.Null(mvm.SelectedTimeSegment);
		}


		[Fact]
		public void SelectedTimeSegmentEndTime_WhenNoTimeSegmentIsSelected_DoesntThrow()
		{
			// Arrange/Act
			var mvm = Helper.GetMainViewModel(string.Empty);

			// Assert
			Assert.Null(mvm.SelectedTimeSegment);
		}

		[Fact]
		public void SelectedTimeSegmentEndTime_WhenNoTimeSegmentIsSelected_IsNotSet()
		{
			// Arrange
			var mvm = Helper.GetMainViewModel(string.Empty);

			// Act
			mvm.SelectedTimeSegmentEndTime = "3/1/2017 10:00 AM";

			// Assert
			Assert.Equal(string.Empty, mvm.SelectedTimeSegmentEndTime);
		}

		[Fact]
		public void IdleTimeSegment_WhenUserIsNotIdle_IsNull()
		{
			// Arrange
			var mockTime = new Mock<IDateTime>();
			var startTime = new DateTime(2017, 3, 1, 10, 0, 0);
			mockTime.Setup(x => x.Now).Returns(startTime);
			var mvm = Helper.GetMainViewModel(0);

			// Assert
			Assert.Null(mvm.IdleTimeSegment);
		}

		[Fact]
		public void IdleTimer_WhenUserIsIdle_Starts()
		{
			// Arrange
			const string idleStartTime = "3/1/2017 10:00:00 AM";
			const string idleEndTime = "3/1/2017 10:15:00 AM";
			var mockTime = new Mock<IDateTime>();
			mockTime.Setup(x => x.Now).Returns(DateTime.Parse(idleEndTime));
			var mockSignalRClient = new Mock<ISignalRClient>();
			mockSignalRClient.Setup(x => x.GetNewestMessage()).Returns(DateTime.Parse(idleStartTime));

			var mvm = Helper.GetMainViewModel(0, mockTime.Object, mockSignalRClient.Object);

			// Act
			mvm.StartStop();
			var isUserIdle = mvm.CheckIdleTime();

			// Assert
			Assert.True(isUserIdle);
			Assert.Equal(idleStartTime, mvm.IdleTimeSegment.StartTime);
			Assert.Equal(idleEndTime, mvm.IdleTimeSegment.EndTime);
			Assert.True(mvm.IsIncludeExcludeEnabled);
		}

		[Fact]
		public void IdleTimer_WhenUserIsStillIdle_HasCorrectStartAndEndTimes()
		{
			// Arrange
			string idleStartTime = "3/1/2017 10:00:00 AM";
			string idleEndTime = "3/1/2017 10:15:00 AM";
			var mockTime = new Mock<IDateTime>();
			mockTime.Setup(x => x.Now).Returns(DateTime.Parse(idleEndTime));
			var mockSignalRClient = new Mock<ISignalRClient>();
			mockSignalRClient.Setup(x => x.GetNewestMessage()).Returns(DateTime.Parse(idleStartTime));

			var mvm = Helper.GetMainViewModel(0, mockTime.Object, mockSignalRClient.Object);

			// Act
			mvm.StartStop();
			var isUserIdle = mvm.CheckIdleTime();

			idleStartTime = idleEndTime;
			idleEndTime = "3/1/2017 10:30:00 AM";
			mockTime.Setup(x => x.Now).Returns(DateTime.Parse(idleEndTime));
			mockSignalRClient.Setup(x => x.GetNewestMessage()).Returns(DateTime.Parse(idleStartTime));
			mvm.CheckIdleTime();

			// Assert
			Assert.True(isUserIdle);
			Assert.Equal(idleStartTime, mvm.IdleTimeSegment.StartTime);
			Assert.Equal(idleEndTime, mvm.IdleTimeSegment.EndTime);
			Assert.True(mvm.IsIncludeExcludeEnabled);
		}

		[Fact]
		public void IdleTimer_WhenUserIdleTimeIsBelowThreshold_DoesNotStart()
		{
			// Arrange
			const string idleStartTime = "3/1/2017 10:00:00 AM";
			const string idleEndTime = "3/1/2017 10:00:01 AM";
			var mockTime = new Mock<IDateTime>();
			mockTime.Setup(x => x.Now).Returns(DateTime.Parse(idleEndTime));
			var mockSignalRClient = new Mock<ISignalRClient>();
			mockSignalRClient.Setup(x => x.GetNewestMessage()).Returns(DateTime.Parse(idleStartTime));

			var mvm = Helper.GetMainViewModel(0, mockTime.Object, mockSignalRClient.Object);

			// Act
			mvm.StartStop();
			var isUserIdle = mvm.CheckIdleTime();

			// Assert
			Assert.False(isUserIdle);
			Assert.False(mvm.IsIncludeExcludeEnabled);
		}

		[Fact]
		public void IdleTimer_WhenUserIsNotIdle_DoesNotStart()
		{
			// Arrange
			var mvm = Helper.GetMainViewModel(0);

			// Act
			mvm.StartStop();
			var isUserIdle = mvm.CheckIdleTime();

			// Assert
			Assert.False(isUserIdle);
			Assert.Null(mvm.IdleTimeSegment);
			Assert.False(mvm.IsIncludeExcludeEnabled);
		}

		[Fact]
		public void IdleTimer_WhenUserIncludesIdleTime_Stops()
		{
			// Arrange
			const string idleStartTime = "3/1/2017 10:00:00 AM";
			const string idleEndTime = "3/1/2017 10:15:00 AM";
			var mockTime = new Mock<IDateTime>();
			mockTime.Setup(x => x.Now).Returns(DateTime.Parse(idleEndTime));
			var mockSignalRClient = new Mock<ISignalRClient>();
			mockSignalRClient.Setup(x => x.GetNewestMessage()).Returns(DateTime.Parse(idleStartTime));

			var mvm = Helper.GetMainViewModel(0, mockTime.Object, mockSignalRClient.Object);

			// Act
			mvm.StartStop();
			mvm.CheckIdleTime();
			mvm.IncludeIdleTime();

			// Assert
			Assert.Null(mvm.IdleTimeSegment);
			Assert.False(mvm.IsIncludeExcludeEnabled);
			Assert.Equal(idleEndTime, mvm.SelectedActivity.ObservableTimeSegments[0].EndTime);
		}

		[Fact]
		public void CurrentTimeSegment_WhenUserExcludesIdleTime_HasCorrectEndTime()
		{
			const string idleStartTime = "3/1/2017 10:00:00 AM";
			const string idleEndTime = "3/1/2017 10:15:00 AM";
			var mockTime = new Mock<IDateTime>();
			mockTime.Setup(x => x.Now).Returns(DateTime.Parse(idleEndTime));
			var mockSignalRClient = new Mock<ISignalRClient>();
			mockSignalRClient.Setup(x => x.GetNewestMessage()).Returns(DateTime.Parse(idleStartTime));

			var mvm = Helper.GetMainViewModel(0, mockTime.Object, mockSignalRClient.Object);

			// Act
			mvm.StartStop();
			mvm.CheckIdleTime();
			mvm.ExcludeIdleTime();

			// Assert
			Assert.Null(mvm.StartedActivity);
			Assert.Null(mvm.IdleTimeSegment);
			Assert.False(mvm.IsIncludeExcludeEnabled);
			Assert.Equal(idleStartTime, mvm.SelectedActivity.ObservableTimeSegments[0].EndTime);
		}

		[Fact]
		public void IncludeExcludeButtons_WhenUserIsIdleAndThenNotIdle_AreEnabled()
		{
			// Arrange
			const string idleStartTime = "3/1/2017 10:00:00 AM";
			const string idleEndTime = "3/1/2017 10:15:00 AM";
			var mockTime = new Mock<IDateTime>();
			mockTime.Setup(x => x.Now).Returns(DateTime.Parse(idleEndTime));
			var mockSignalRClient = new Mock<ISignalRClient>();
			mockSignalRClient.Setup(x => x.GetNewestMessage()).Returns(DateTime.Parse(idleStartTime));

			var mvm = Helper.GetMainViewModel(0, mockTime.Object, mockSignalRClient.Object);

			// Act
			mvm.StartStop();
			Assert.False(mvm.IsIncludeExcludeEnabled);
			var isIdle = mvm.CheckIdleTime();
			Assert.True(isIdle);
			Assert.True(mvm.IsIncludeExcludeEnabled);
			mockSignalRClient.Setup(x => x.GetNewestMessage()).Returns(DateTime.Parse(idleEndTime));
			isIdle = mvm.CheckIdleTime();
			Assert.False(isIdle);

			// Assert
			Assert.True(mvm.IsIncludeExcludeEnabled);
		}
	}
}
