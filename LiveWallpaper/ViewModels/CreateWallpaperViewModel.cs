﻿using Caliburn.Micro;
using System.Diagnostics;
using MultiLanguageForXAML;
using LiveWallpaper.WallpaperManagers;
using LiveWallpaper.WallpaperManagers.Controls;
//using LiveWallpaperEngineLib.NativeWallpapers;
using System.Windows.Interop;
using LiveWallpaper.Managers;
using System.Threading.Tasks;
using System.Windows;
using System.Text;
using System.IO;
using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LiveWallpaperEngineAPI;
using LiveWallpaperEngineAPI.Models;
using System.Collections.Generic;
using System.Linq;

namespace LiveWallpaper.ViewModels
{
    public class CreateWallpaperViewModel : ScreenWindow
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private bool _editMode;

        //默认是false，修改后内存保存
        private bool _preview;
        private Dictionary<uint, WallpaperModel> _beforePreviewModel;

        public CreateWallpaperViewModel()
        {
            DisplayName = LanService.Get("common_create").Result;
            PreviewWallpaper = _preview;
        }

        #region properties

        public bool Result { get; set; }

        #region CurrentWallpaper

        /// <summary>
        /// The <see cref="CurrentWallpaper" /> property's name.
        /// </summary>
        public const string CurrentWallpaperPropertyName = "CurrentWallpaper";

        private Wallpaper _CurrentWallpaper;
        private Wallpaper _OldWallpaper;

        /// <summary>
        /// CurrentWallpaper
        /// </summary>
        public Wallpaper CurrentWallpaper
        {
            get { return _CurrentWallpaper; }

            set
            {
                if (_CurrentWallpaper == value) return;

                if (_OldWallpaper == null)
                    _OldWallpaper = _CurrentWallpaper;
                _CurrentWallpaper = value;
                if (_preview)
                    Preview();

                NotifyOfPropertyChange(CurrentWallpaperPropertyName);
            }
        }

        #endregion

        #region PreviewWallpaper

        /// <summary>
        /// The <see cref="PreviewWallpaper" /> property's name.
        /// </summary>
        public const string PreviewWallpaperPropertyName = "PreviewWallpaper";

        private bool _PreviewWallpaper;

        /// <summary>
        /// PreviewWallpaper
        /// </summary>
        public bool PreviewWallpaper
        {
            get { return _PreviewWallpaper; }

            set
            {
                if (_PreviewWallpaper == value) return;

                _PreviewWallpaper = value;
                NotifyOfPropertyChange(PreviewWallpaperPropertyName);
            }
        }

        #endregion

        #region FilePath

        /// <summary>
        /// The <see cref="FilePath" /> property's name.
        /// </summary>
        public const string FilePathPropertyName = "FilePath";

        private string _FilePath;

        /// <summary>
        /// FilePath
        /// </summary>
        public string FilePath
        {
            get { return _FilePath; }

            set
            {
                if (_FilePath == value) return;

                _FilePath = value;
                NotifyOfPropertyChange(FilePathPropertyName);
            }
        }

        #endregion

        #region CanSave

        /// <summary>
        /// The <see cref="CanSave" /> property's name.
        /// </summary>
        public const string CanSavePropertyName = "CanSave";

        private bool _CanSave = true;

        /// <summary>
        /// CanSave
        /// </summary>
        public bool CanSave
        {
            get { return _CanSave; }

            set
            {
                if (_CanSave == value) return;

                _CanSave = value;
                NotifyOfPropertyChange(CanSavePropertyName);
            }
        }

        #endregion

        #endregion

        #region public methods

        public async void SelectFile()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(await LanService.Get("wallpaperEditor_fileDialogType"));
            foreach (var item in Wallpaper.VideoExtensions)
            {
                sb.Append($"{item};");
            }
            var openFileDialog = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = sb.ToString()
            };
            var result = openFileDialog.ShowDialog();
            if (result == true)
            {
                FilePath = openFileDialog.FileName;
            }
        }

        internal void SetPaper(Wallpaper w)
        {
            DisplayName = LanService.Get("common_edit").Result;
            CurrentWallpaper = w;
            _editMode = true;
        }

        public async void Preview()
        {
            if (CurrentWallpaper != null)
            {
                _preview = true;
                //所有屏幕一起预览
                _beforePreviewModel = new Dictionary<uint, WallpaperModel>(WallpaperManager.Instance.CurrentWalpapers);
                await WallpaperManager.Instance.ShowWallpaper(new WallpaperModel()
                {
                    Path = CurrentWallpaper.AbsolutePath
                }, WallpaperManager.Instance.ScreenIndexs);
            }
        }

        public async void StopPreview()
        {
            if (_preview)
            {
                uint[] unuseScreen = WallpaperManager.Instance.ScreenIndexs.ToList().Except(_beforePreviewModel.Keys.ToList()).ToArray();
                WallpaperManager.Instance.CloseWallpaper(unuseScreen);
                foreach (var (screenIndex, wallpaper) in _beforePreviewModel)
                {
                    //恢复
                    await WallpaperManager.Instance.ShowWallpaper(wallpaper, screenIndex);
                }
            }
            _preview = false;
        }

        public void Cancel()
        {
            CurrentWallpaper = null;
            //mpv player有时候导致卡死不明原因，曲线救国
            //await Task.Delay(1000);
            StopPreview();
            Result = false;
            TryClose();
        }

        public async void Save()
        {
            if (CurrentWallpaper == null)
            {
                MessageBox.Show(await LanService.Get("wallpaperEditor_warning_invalidWallpaper"));
                return;
            }
            if (string.IsNullOrEmpty(CurrentWallpaper.ProjectInfo.Title))
            {
                MessageBox.Show(await LanService.Get("wallpaperEditor_warning_titleEmpty"));
                return;
            }

            CanSave = false;

            try
            {
                if (_editMode)
                {
                    if (_OldWallpaper == null)
                        //只修改了title，desc等信息
                        _OldWallpaper = CurrentWallpaper;
                    await Wallpaper.EditLocakPack(CurrentWallpaper, _OldWallpaper.Dir);
                }
                else
                {
                    string destDir; destDir = Path.Combine(AppManager.LocalWallpaperDir, Guid.NewGuid().ToString());
                    await Wallpaper.CreateLocalPack(CurrentWallpaper, destDir);
                }
                //if (_editMode)
                //{
                //    //删除旧包
                //    var temp = CurrentWallpaper;
                //    CurrentWallpaper = null;
                //    bool ok = await Wallpaper.Delete(temp);
                //    if (!ok)
                //    {
                //        MessageBox.Show("删除失败请手动删除");
                //    }
                //}
            }
            catch (Exception ex)
            {
                CanSave = true;
                logger.Error(ex);
                MessageBox.Show(ex.Message);
                return;
            }
            Result = true;
            TryClose();
        }

        public void GeneratePreview(object parameter)
        {
            if (!(parameter is WallpaperRender render) || CurrentWallpaper == null)
                return;

            try
            {
                var tmpImg = Path.GetTempFileName().Replace(".tmp", ".png");
                using (FileStream stream = File.Open(tmpImg, FileMode.Create))
                {
                    RenderTargetBitmap bmp = new RenderTargetBitmap((int)render.ActualWidth,
                        (int)render.ActualHeight, 96, 96, PixelFormats.Pbgra32);

                    bmp.Render(render);

                    //用16：9
                    //var width = (int)(render.ActualWidth > render.ActualHeight ? render.ActualHeight : render.ActualWidth);
                    //var height = (int)(9 / 16.0 * width);
                    //var x = (int)(render.ActualWidth / 2 - width / 2);
                    //var y = (int)(render.ActualHeight / 2 - height / 2);
                    int width = (int)render.ActualWidth;
                    int height = (int)render.ActualHeight;
                    int x = 0;
                    int y = 0;
                    CroppedBitmap crop = new CroppedBitmap(bmp, new Int32Rect(x, y, width, height));

                    PngBitmapEncoder coder = new PngBitmapEncoder
                    {
                        Interlace = PngInterlaceOption.Off
                    };
                    coder.Frames.Add(BitmapFrame.Create(crop));
                    coder.Save(stream);
                }

                CurrentWallpaper.AbsolutePreviewPath = tmpImg;

                //CurrentWallpaper.NotifyOfPropertyChange(Wallpaper.AbsolutePreviewPathPropertyName);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                MessageBox.Show(ex.Message);
            }
        }

        //public void GeneratePreview(object parameter)
        //{
        //    if (!(parameter is WallpaperRender render) || CurrentWallpaper == null)
        //        return;

        //    try
        //    {
        //        var tmpImg = Path.GetTempFileName();
        //        render.Capture(tmpImg);
        //        CurrentWallpaper.AbsolutePreviewPath = tmpImg;
        //        //using (FileStream stream = File.Open(tmpImg, FileMode.Create))
        //        //{
        //        //    RenderTargetBitmap bmp = new RenderTargetBitmap((int)render.ActualWidth,
        //        //        (int)render.ActualHeight, 96, 96, PixelFormats.Pbgra32);

        //        //    bmp.Render(render);

        //        //    int width = (int)render.ActualWidth;
        //        //    int height = (int)render.ActualHeight;
        //        //    int x = 0;
        //        //    int y = 0;
        //        //    CroppedBitmap crop = new CroppedBitmap(bmp, new Int32Rect(x, y, width, height));

        //        //    PngBitmapEncoder coder = new PngBitmapEncoder
        //        //    {
        //        //        Interlace = PngInterlaceOption.Off
        //        //    };
        //        //    coder.Frames.Add(BitmapFrame.Create(crop));
        //        //    coder.Save(stream);
        //        //}

        //        //CurrentWallpaper.AbsolutePreviewPath = tmpImg;
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex);
        //        MessageBox.Show(ex.Message);
        //    }
        //}

        //public void RestartExploer()
        //{
        //    foreach (Process exe in Process.GetProcesses())
        //    {
        //        if (exe.ProcessName.StartsWith("explorer"))
        //        {
        //            exe.Kill();
        //            break;
        //        }
        //    }

        //    var p = new Process();
        //    string explorer = "explorer.exe";
        //    p.StartInfo.FileName = explorer;
        //    p.Start();
        //    p.Kill();
        //}

        #endregion

        #region private methods

        #endregion
    }
}
