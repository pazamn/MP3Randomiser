using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows;

namespace FileRandomiser.Window
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        #region Private Fields

        private string _sourceFolder;
        private string _targetFolder;
        private bool _clearBefore;
        private bool _splitByFolders;
        private int _splitBy;
        private bool _clearId3Tags;
        private string _copyingFormats;

        private Visibility _textBlockVisibility;
        private Visibility _progressBarVisibility;
        private bool _infiniteProgress;
        private int _progress;
        private int _maxValue;

        private bool _isEnabled;

        #endregion Private Fields

        #region Public Properties

        public string SourceFolder
        {
            get { return _sourceFolder; }
            set { SetField(ref _sourceFolder, value, () => SourceFolder); }
        }

        public string TargetFolder
        {
            get { return _targetFolder; }
            set { SetField(ref _targetFolder, value, () => TargetFolder); }
        }

        public bool ClearBefore
        {
            get { return _clearBefore; }
            set { SetField(ref _clearBefore, value, () => ClearBefore); }
        }

        public bool SplitByFolders
        {
            get { return _splitByFolders; }
            set { SetField(ref _splitByFolders, value, () => SplitByFolders); }
        }

        public int SplitBy
        {
            get { return _splitBy; }
            set { SetField(ref _splitBy, value, () => SplitBy); }
        }

        public bool ClearID3Tags
        {
            get { return _clearId3Tags; }
            set { SetField(ref _clearId3Tags, value, () => ClearID3Tags); }
        }

        public string CopyingFormats
        {
            get { return _copyingFormats; }
            set { SetField(ref _copyingFormats, value, () => CopyingFormats); }
        }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetField(ref _isEnabled, value, () => IsEnabled); }
        }

        public bool InfiniteProgress
        {
            get { return _infiniteProgress; }
            set { SetField(ref _infiniteProgress, value, () => InfiniteProgress); }
        }

        public int Progress
        {
            get { return _progress; }
            set { SetField(ref _progress, value, () => Progress); }
        }

        public int MaxValue
        {
            get { return _maxValue; }
            set { SetField(ref _maxValue, value, () => MaxValue); }
        }

        public Visibility TextBlockVisibility
        {
            get { return _textBlockVisibility; }
            set { SetField(ref _textBlockVisibility, value, () => TextBlockVisibility); }
        }

        public Visibility ProgressBarVisibility
        {
            get { return _progressBarVisibility; }
            set { SetField(ref _progressBarVisibility, value, () => ProgressBarVisibility); }
        }

        #endregion Public Properties

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected bool SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged<T>(Expression<Func<T>> selectorExpression)
        {
            if (selectorExpression == null)
            {
                throw new ArgumentNullException("selectorExpression");
            }

            var body = selectorExpression.Body as MemberExpression;
            if (body == null)
            {
                throw new ArgumentException("The body must be a member expression");
            }

            OnPropertyChanged(body.Member.Name);
        }

        protected bool SetField<T>(ref T field, T value, Expression<Func<T>> selectorExpression)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(selectorExpression);
            return true;
        }

        #endregion INotifyPropertyChanged Implementation
    }
}