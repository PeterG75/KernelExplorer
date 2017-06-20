﻿using JobView.Models;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JobView.ViewModels {
	class MainViewModel : BindableBase, IMainViewModel, IDisposable {
		JobManager _jobManager;
		DriverInterface _driver;
		List<JobObjectViewModel> _rootJobs;
		Dictionary<UIntPtr, JobObjectViewModel> _jobs;

		public DriverInterface Driver => _driver;

		public JobDetailsViewModel JobDetails { get; } 

		public MainViewModel() {
			_jobManager = new JobManager();
			_driver = new DriverInterface();
			JobDetails = new JobDetailsViewModel(this);
			Refresh();
		}


		public void Dispose() {
			_driver.Dispose();
		}

		public IEnumerable<JobObjectViewModel> RootJobs => _rootJobs;

		public ICommand RefreshCommand => new DelegateCommand(() => Refresh());

		private void Refresh() {
			_jobManager.BuildJobTree(_driver);
			_jobs = _jobManager.AllJobs.Select(job => new JobObjectViewModel(job)).ToDictionary(job => job.Job.Address);
			_rootJobs = _jobs.Values.Where(job => job.ParentJob == null).ToList();
			foreach (var job in _jobs.Values.Where(job => job.Job.ChildJobs != null)) {
				job.ChildJobs = job.Job.ChildJobs.Select(child => new JobObjectViewModel(child)).ToList();
			}

			RaisePropertyChanged(nameof(RootJobs));
		}

		public JobObjectViewModel GetJobByAddress(UIntPtr address) {
			return _jobs[address];
		}

		private JobObjectViewModel _selectedJob;

		public JobObjectViewModel SelectedJob {
			get { return _selectedJob; }
			set {
				if (SetProperty(ref _selectedJob, value)) {
					JobDetails.Job = value;
					_selectedJob.IsSelected = true;
				}
			}
		}

	}
}