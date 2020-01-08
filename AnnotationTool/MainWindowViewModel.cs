using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace AnnotationTool
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        public MainWindowViewModel()
        {
            FrameInformations = new ObservableCollection<FrameInformation>();
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string KeyOfMoveUp
        {
            get { return _keyOfMoveUp; }
            set
            {
                if (value != _keyOfMoveUp)
                {
                    _keyOfMoveUp = value;
                    OnPropertyChanged("KeyOfMoveUp");
                }
            }
        }

        private string _keyOfMoveUp;

        public string KeyOfMoveDown
        {
            get { return _keyOfMoveDown; }
            set
            {
                if (value != _keyOfMoveDown)
                {
                    _keyOfMoveDown = value;
                    OnPropertyChanged("KeyOfMoveDown");
                }
            }
        }

        private string _keyOfMoveDown;

        public string KeyOfMoveLeft
        {
            get { return _keyOfMoveLeft; }
            set
            {
                if (value != _keyOfMoveLeft)
                {
                    _keyOfMoveLeft = value;

                    OnPropertyChanged("KeyOfMoveLeft");
                }
            }
        }

        private string _keyOfMoveLeft;

        public string KeyOfMoveRight
        {
            get { return _keyOfMoveRight; }
            set
            {
                if (value != _keyOfMoveRight)
                {
                    _keyOfMoveRight = value;

                    OnPropertyChanged("KeyOfMoveRight");
                }
            }
        }

        private string _keyOfMoveRight;

        public string KeyOfExpandUpDown
        {
            get { return _keyOfExpandUpDown; }
            set
            {
                if (value != _keyOfExpandUpDown)
                {
                    _keyOfExpandUpDown = value;

                    OnPropertyChanged("KeyOfExpandUpDown");
                }
            }
        }

        private string _keyOfExpandUpDown;

        public string KeyOfShrinkUpDown
        {
            get { return _keyOfShrinkUpDown; }
            set
            {
                if (value != _keyOfShrinkUpDown)
                {
                    _keyOfShrinkUpDown = value;

                    OnPropertyChanged("KeyOfShrinkUpDown");
                }
            }
        }

        private string _keyOfShrinkUpDown;

        public string KeyOfExpandLeftRight
        {
            get { return _keyOfExpandLeftRight; }
            set
            {
                if (value != _keyOfExpandLeftRight)
                {
                    _keyOfExpandLeftRight = value;

                    OnPropertyChanged("KeyOfExpandLeftRight");
                }
            }
        }

        private string _keyOfExpandLeftRight;

        public string KeyOfShinkLeftRight
        {
            get { return _keyOfShinkLeftRight; }
            set
            {
                if (value != _keyOfShinkLeftRight)
                {
                    _keyOfShinkLeftRight = value;

                    OnPropertyChanged("KeyOfShinkLeftRight");
                }
            }
        }

        private string _keyOfShinkLeftRight;

        public string KeyOfFullOcclusion
        {
            get
            {
                if (_keyOfFullOcclusion == null) return "";
                return _keyOfFullOcclusion;
            }
            set
            {
                if (value != _keyOfFullOcclusion)
                {
                    _keyOfFullOcclusion = value;
                    OnPropertyChanged("KeyOfFullOcclusion");
                }
            }
        }
        private string _keyOfFullOcclusion;

        public string KeyOfOutOfView
        {
            get { return _keyOfOutOfView; }
            set
            {
                if (value != _keyOfOutOfView)
                {
                    _keyOfOutOfView = value;
                    OnPropertyChanged("KeyOfOutOfView");
                }
            }
        }
        private string _keyOfOutOfView;

        public string KeyPreviousFrame
        {
            get { return _keyPreviousFrame; }
            set
            {
                if (value != _keyPreviousFrame)
                {
                    _keyPreviousFrame = value;
                    OnPropertyChanged("KeyPreviousFrame");
                }
            }
        }

        private string _keyPreviousFrame;

        public string KeyNextFrame
        {
            get { return _keyNextFrame; }
            set
            {
                if (_keyNextFrame != value)
                {
                    _keyNextFrame = value;
                    OnPropertyChanged("KeyNextFrame");
                }
            }
        }
        private string _keyNextFrame;

        public string KeySubmit
        {
            get { return _keySubmit; }
            set
            {
                if (_keySubmit != value)
                {
                    _keySubmit = value;
                    OnPropertyChanged("KeySubmit");
                }
            }
        }

        private string _keySubmit;

        public struct FrameInformation : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            // This method is called by the Set accessor of each property.
            // The CallerMemberName attribute that is applied to the optional propertyName
            // parameter causes the property name of the caller to be substituted as an argument.
            private void NotifyPropertyChanged(String propertyName)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }

            public int Id
            {
                get { return _id; }
                set
                {
                    if (_id != value)
                    {
                        _id = value;
                        NotifyPropertyChanged("Id");
                    }
                }
            }

            private int _id;

            public bool IsLabeled
            {
                get { return _isLabeled; }
                set
                {
                    if (_isLabeled != value)
                    {
                        _isLabeled = value;
                        NotifyPropertyChanged("IsLabeled");
                    }
                }
            }

            private bool _isLabeled;
            public int X
            {
                get { return _x; }
                set
                {
                    if (_x != value)
                    {
                        _x = value;
                        NotifyPropertyChanged("X");
                    }
                }
            }
            private int _x;

            public int Y
            {
                get { return _y; }
                set
                {
                    if (_y != value)
                    {
                        _y = value;
                        NotifyPropertyChanged("Y");
                    }
                }
            }

            private int _y;

            public int W
            {
                get { return _w; }
                set
                {
                    if (_w != value)
                    {
                        _w = value;
                        NotifyPropertyChanged("W");
                    }
                }
            }

            private int _w;

            public int H
            {
                get { return _h; }
                set
                {
                    if (_h != value)
                    {
                        _h = value;
                        NotifyPropertyChanged("H");
                    }
                }
            }

            private int _h;

            public bool IsFullOccluded
            {
                get { return _isFullOccluded; }
                set
                {
                    if (_isFullOccluded != value)
                    {
                        _isFullOccluded = value;
                        NotifyPropertyChanged("IsFullOccluded");
                    }
                }
            }

            private bool _isFullOccluded;

            public bool IsOutOfView
            {
                get { return _isOutOfView; }
                set
                {
                    if (_isOutOfView != value)
                    {
                        _isOutOfView = value;
                        NotifyPropertyChanged("IsOutOfView");
                    }
                }
            }

            private bool _isOutOfView;
        }

        public ObservableCollection<FrameInformation> FrameInformations
        {
            get { return _frameInformations; }
            set
            {
                if (_frameInformations != value)
                {
                    _frameInformations = value;
                    OnPropertyChanged("FrameInformations");
                }
            }
        }

        private ObservableCollection<FrameInformation> _frameInformations;

        public string CurrentSequenceName
        {
            get { return _currentSequenceName; }
            set
            {
                if (value != _currentSequenceName)
                {
                    _currentSequenceName = value;
                    OnPropertyChanged("CurrentSequenceName");
                }
            }
        }
        private string _currentSequenceName;

        public int CurrentIndexOfFrame
        {
            get { return _currentIndexOfFrame; }
            set
            {
                if (_currentIndexOfFrame != value + 1)
                {
                    _currentIndexOfFrame = value + 1;
                    OnPropertyChanged("CurrentIndexOfFrame");
                }
            }
        }

        private int _currentIndexOfFrame = 0;
        public int TotalNumberOfFrames {
            get { return _totalNumberOfFrames; }
            set
            {
                if (_totalNumberOfFrames != value)
                {
                    _totalNumberOfFrames = value;
                    OnPropertyChanged("TotalNumberOfFrames");
                }
            } }
        private int _totalNumberOfFrames = 0;

        public bool IsTrackerActivated
        {
            get { return _isTrackerActivated; }
            set
            {
                if (value != _isTrackerActivated)
                {
                    _isTrackerActivated = value;
                    OnPropertyChanged("IsTrackerActivated");
                }
            }
        }

        private bool _isTrackerActivated;

        public bool IsUsingTrackerOnAnnotatedRecord
        {
            get { return _isUsingTrackerOnAnnotatedRecord; }
            set
            {
                if (value != _isUsingTrackerOnAnnotatedRecord)
                {
                    _isUsingTrackerOnAnnotatedRecord = value;
                    OnPropertyChanged("IsUsingTrackerOnAnnotatedRecord");
                }
            }
        }

        private bool _isUsingTrackerOnAnnotatedRecord;

        public bool IsUpdateMatlabRecordOnSubmit
        {
            get { return _isUpdateMatlabRecordOnSubmit; }
            set
            {
                if (value != _isUpdateMatlabRecordOnSubmit)
                {
                    _isUpdateMatlabRecordOnSubmit = value;
                    OnPropertyChanged("IsUpdateMatlabRecordOnSubmit");
                }
            }
        }

        private bool _isUpdateMatlabRecordOnSubmit;
        
        public bool IsUpdateMatlabRecordOnAppropriateOpportunity
        {
            get { return _isUpdateMatlabRecordOnAppropriateOpportunity; }
            set
            {
                if (value != _isUpdateMatlabRecordOnAppropriateOpportunity)
                {
                    _isUpdateMatlabRecordOnAppropriateOpportunity = value;
                    OnPropertyChanged("IsUpdateMatlabRecordOnAppropriateOpportunity");
                }
            }
        }

        private bool _isUpdateMatlabRecordOnAppropriateOpportunity;
        
        public void Reset()
        {
            IsFullyOccluded = false;
            IsOutOfView = false;
            _currentIndexOfFrame = 0;
            OnPropertyChanged("CurrentIndexOfFrame");
            TotalNumberOfFrames = 0;
            FrameInformations.Clear();
        }

        public int FindUnlabeledFrameIndex()
        {
            int index = 0;
            foreach (var frameInformation in FrameInformations)
            {
                if (!frameInformation.IsLabeled)
                    break;
                ++index;
            }

            return index;
        }

        public int X
        {
            get { return _x; }
            set
            {
                if (_x != value)
                {
                    _x = value;
                    OnPropertyChanged("X");
                }
            }
        }
        private int _x;

        public int Y
        {
            get { return _y; }
            set
            {
                if (_y != value)
                {
                    _y = value;
                    OnPropertyChanged("Y");
                }
            }
        }

        private int _y;

        public int W
        {
            get { return _w; }
            set
            {
                if (_w != value)
                {
                    _w = value;
                    OnPropertyChanged("W");
                }
            }
        }

        private int _w;

        public int H
        {
            get { return _h; }
            set
            {
                if (_h != value)
                {
                    _h = value;
                    OnPropertyChanged("H");
                }
            }
        }

        private int _h;

        public bool IsFullyOccluded
        {
            get { return _isFullyOccluded; }
            set
            {
                if (_isFullyOccluded != value)
                {
                    _isFullyOccluded = value;
                    OnPropertyChanged("IsFullyOccluded");
                }
            }
        }

        private bool _isFullyOccluded;

        public bool IsOutOfView
        {
            get { return _isOutOfView; }
            set
            {
                if (_isOutOfView != value)
                {
                    _isOutOfView = value;
                    OnPropertyChanged("IsOutOfView");
                }
            }
        }

        private bool _isOutOfView;

        public int MaxIndexOfFrame
        {
            get { return _maxIndexOfFrame; }
            set
            {
                if (_maxIndexOfFrame != value)
                {
                    _maxIndexOfFrame = value;
                    OnPropertyChanged("MaxIndexOfFrame");
                }
            }
        }

        private int _maxIndexOfFrame = 1;

        public string KeyOfReTrack
        {
            get { return _keyOfReTrack; }
            set
            {
                if (_keyOfReTrack != value)
                {
                    _keyOfReTrack = value;
                    OnPropertyChanged("KeyOfReTrack");
                }
            }
        }

        private string _keyOfReTrack;
        public int MaximumOfX
        {
            get { return _maximumOfX; }
            set
            {
                if (_maximumOfX != value)
                {
                    _maximumOfX = value;
                    OnPropertyChanged("MaximumOfX");
                }
            }
        }

        private int _maximumOfX;
        public int MaximumOfY
        {
            get { return _maximumOfY; }
            set
            {
                if (_maximumOfY != value)
                {
                    _maximumOfY = value;
                    OnPropertyChanged("MaximumOfY");
                }
            }
        }

        private int _maximumOfY;
        public int MaximumOfW
        {
            get { return _maximumOfW; }
            set
            {
                if (_maximumOfW != value)
                {
                    _maximumOfW = value;
                    OnPropertyChanged("MaximumOfW");
                }
            }
        }

        private int _maximumOfW;
        public int MaximumOfH
        {
            get { return _maximumOfH; }
            set
            {
                if (_maximumOfH != value)
                {
                    _maximumOfH = value;
                    OnPropertyChanged("MaximumOfH");
                }
            }
        }

        private int _maximumOfH;
        
        public string KeyOfCopyLastFrame
        {
            get { return _keyOfCopyLastFrame; }
            set
            {
                if (_keyOfCopyLastFrame != value)
                {
                    _keyOfCopyLastFrame = value;
                    OnPropertyChanged("KeyOfCopyLastFrame");
                }
            }
        }

        private string _keyOfCopyLastFrame;

        public string KeyOfDeleteRecord
        {
            get { return _keyOfDeleteRecord; }
            set
            {
                if (_keyOfDeleteRecord != value)
                {
                    _keyOfDeleteRecord = value;
                    OnPropertyChanged("KeyOfDeleteRecord");
                }
            }
        }

        private string _keyOfDeleteRecord;
    }
}
