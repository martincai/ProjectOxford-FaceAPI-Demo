// *********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
//
// *********************************************************
namespace Microsoft.ProjectOxford.Face.Controls
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;

    using Microsoft.ProjectOxford.Face;
    using System.Linq;
    using System;

    /// <summary>
    /// Interaction logic for FaceDetection.xaml
    /// </summary>
    public partial class FaceDetection : UserControl, INotifyPropertyChanged
    {
        #region Fields

        /// <summary>
        /// Description dependency property
        /// </summary>
        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(FaceDetection));

        /// <summary>
        /// Output dependency property
        /// </summary>
        public static readonly DependencyProperty OutputProperty = DependencyProperty.Register("Output", typeof(string), typeof(FaceDetection));

        /// <summary>
        /// Face detection results in list container
        /// </summary>
        private ObservableCollection<Face> _detectedFaces = new ObservableCollection<Face>();

        /// <summary>
        /// Face detection results in text string
        /// </summary>
        private string _detectedResultsInText;

        /// <summary>
        /// Face detection results container
        /// </summary>
        private ObservableCollection<Face> _resultCollection = new ObservableCollection<Face>();

        /// <summary>
        /// Image path used for rendering and detecting
        /// </summary>
        private string _selectedFile;

        private static Boolean greenLight = true;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FaceDetection" /> class
        /// </summary>
        public FaceDetection()
        {
            InitializeComponent();
        }

        #endregion Constructors

        #region Events

        /// <summary>
        /// Implement INotifyPropertyChanged event handler
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets or sets description
        /// </summary>
        public string Description
        {
            get
            {
                return (string)GetValue(DescriptionProperty);
            }

            set
            {
                SetValue(DescriptionProperty, value);
            }
        }

        /// <summary>
        /// Gets face detection results
        /// </summary>
        public ObservableCollection<Face> DetectedFaces
        {
            get
            {
                return _detectedFaces;
            }
        }

        /// <summary>
        /// Gets or sets face detection results in text string
        /// </summary>
        public string DetectedResultsInText
        {
            get
            {
                return _detectedResultsInText;
            }

            set
            {
                _detectedResultsInText = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("DetectedResultsInText"));
                }
            }
        }

        /// <summary>
        /// Gets constant maximum image size for rendering detection result
        /// </summary>
        public int MaxImageSize
        {
            get
            {
                //return 300;
                return 640;
            }
        }

        /// <summary>
        /// Gets or sets output for rendering
        /// </summary>
        public string Output
        {
            get
            {
                return (string)GetValue(OutputProperty);
            }

            set
            {
                SetValue(OutputProperty, value);
            }
        }

        /// <summary>
        /// Gets face detection results
        /// </summary>
        public ObservableCollection<Face> ResultCollection
        {
            get
            {
                return _resultCollection;
            }
        }

        /// <summary>
        /// Gets or sets image path for rendering and detecting
        /// </summary>
        public string SelectedFile
        {
            get
            {
                return _selectedFile;
            }

            set
            {
                _selectedFile = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("SelectedFile"));
                }
            }
        }

        #endregion Properties

        #region Methods

        // Get latest file from cameratoll directory
        private void GetLatestImage()
        {

        }

        /// <summary>
        /// Pick image for face detection and set detection result to result container
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event argument</param>
        private async void ImagePicker_Click(object sender, RoutedEventArgs e)
        {
            var directory = new DirectoryInfo("C:\\Users\\mcai\\Pictures\\Camera Roll");
            string myFile = "";
            //string myLastFile = "";

            //while (true)
            {
                try
                {
                    //                    var myFileInfo = directory.GetFiles("*.jpg").OrderByDescending(f => f.LastWriteTime).First();
                    myFile = directory.GetFiles("*.jpg").OrderByDescending(f => f.LastWriteTime).First().FullName;
                }
                catch (Exception ex)
                {
                    myFile = "";
                }

                //                if (myFileInfo != null)
                if (myFile != "")
                {
                    //myFile = myFileInfo.FullName;
                    //System.Threading.Thread.Sleep(1000);
                    //greenLight = false;

                    // Show file picker dialog
                    //            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                    //            dlg.DefaultExt = ".jpg";
                    //            dlg.Filter = "Image files(*.jpg) | *.jpg";
                    //            var result = dlg.ShowDialog();

                    //            if (result.HasValue && result.Value)
                    //            {
                    // User picked one image
                    //  var imageInfo = UIHelper.GetImageInfoForRendering(dlg.FileName);
                    var imageInfo = UIHelper.GetImageInfoForRendering(myFile);
                    //  SelectedFile = dlg.FileName;
                    SelectedFile = myFile;

                    // Clear last detection result
                    ResultCollection.Clear();
                    DetectedFaces.Clear();
                    DetectedResultsInText = string.Format("Detecting...");

                    Output = Output.AppendLine(string.Format("Request: Detecting {0}", SelectedFile));

                    var sw = Stopwatch.StartNew();

                    // Call detection REST API
                    using (var fileStream = File.OpenRead(SelectedFile))
                    {
                        try
                        {
                            var faces = await App.Instance.DetectAsync(fileStream, false, true, true, false);
                            Output = Output.AppendLine(string.Format("Response: Success. Detected {0} face(s) in {1}", faces.Length, SelectedFile));

                            DetectedResultsInText = string.Format("{0} face(s) has been detected", faces.Length);

                            foreach (var face in faces)
                            {
                                String chsGender;
                                if (face.Attributes.Gender == "male")
                                    chsGender = "男";
                                else
                                    chsGender = "女";

                                DetectedFaces.Add(new Face()
                                {
                                    ImagePath = SelectedFile,
                                    Left = face.FaceRectangle.Left,
                                    Top = face.FaceRectangle.Top,
                                    Width = face.FaceRectangle.Width,
                                    Height = face.FaceRectangle.Height,
                                    FaceId = face.FaceId.ToString(),
                                    Gender = chsGender,
                                    //Gender = face.Attributes.Gender,
                                    //Age = string.Format("{0:#} years old", face.Attributes.Age),
                                    Age = string.Format("{0:#}岁", face.Attributes.Age),
                                });
                            }

                            // Convert detection result into UI binding object for rendering
                            foreach (var face in UIHelper.CalculateFaceRectangleForRendering(faces, MaxImageSize, imageInfo))
                            {
                                ResultCollection.Add(face);
                            }

                            //greenLight = true;

                            // Clean up
                            /*if (myLastFile != "")
                            {
                                File.Delete(myLastFile);
                            }
                            myLastFile = myFile;*/
                        }
                        catch (ClientException ex)
                        {
                            Output = Output.AppendLine(string.Format("Response: {0}. {1}", ex.Error.Code, ex.Error.Message));
                            return;
                        }
                    }
                    //            }
                }
            }
        }

        #endregion Methods
    }
}