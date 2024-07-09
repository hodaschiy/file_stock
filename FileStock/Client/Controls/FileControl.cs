﻿using Newtonsoft.Json;
using System;
using System.IO;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Windows.Shapes;
using NLog;
using System.Net.Http;

namespace Client.Controls
{
    public class FileControl
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        public List<FileModel> GetUserFiles()
        {
            try
            {
                return App.http.GetFromJsonAsAsyncEnumerable<FileModel>($"/FileModels/Get?Login={App.usr.Name}&Token={App.usr.Token}").ToListAsync<FileModel>().Result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return new List<FileModel>();
            }

        }
        public async Task<FileModel> Add() 
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Multiselect = false; // явное указание, что выбирается только 1 файл, потом можно доработать на мультивыбор

            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                try
                {
                    CompressAlg alg = CompressAlg.GZip;
                    _logger.Info($"Start add file: {dlg.SafeFileName}");
                    StreamContent streamContent = new StreamContent(new MemoryStream(App.archiver.Compress(File.OpenRead(dlg.FileName), alg)));
                    StringContent stringContent = new StringContent(((int)alg).ToString());
                    MultipartFormDataContent form = new MultipartFormDataContent();
                    form.Add(streamContent, "First", dlg.SafeFileName);
                    form.Add(stringContent, "CompressAlg");

                    var res = await App.http.PostAsync("/FileModels/Add", form);
                    var fl = await res.Content.ReadFromJsonAsync<FileModel>();
                    if (fl == FileModel.Empty)
                    {
                        _logger.Error("File not exists");
                        return null;
                    }
                    _logger.Info($"Add file: FileId: {fl.Id}; FileName: {fl.Name}; FileSize: {fl.Size}; CompressionAlghoritm: {fl.CompressAlg}");
                    return fl;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                    return null;
                }
            }
            else
            { 
                return null;
            }
        }
        public async Task<bool> Remove(int id)
        {
            JsonContent jsonContent = JsonContent.Create(new { Id = id});
            var res = await App.http.PostAsync("/FileModels/Delete", jsonContent); 
            var IsDeleted = await res.Content.ReadFromJsonAsync<bool>();
            return IsDeleted;
        }
        public async Task<bool> Download(FileModel savingFile)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            string path = dialog.SelectedPath;

            if (path != null && path != String.Empty)
            {
                try
                {
                    JsonContent jsonContent = JsonContent.Create(new { Id = savingFile.Id });
                    var res = await App.http.PostAsync("/FileModels/Download", jsonContent);
                    byte[] data = App.archiver.Decompress(await res.Content.ReadAsStreamAsync(), savingFile.CompressAlg);
                    File.WriteAllBytes(path + '/' + savingFile.Name, data);

                    return true;
                }
                catch (Exception ex) 
                {
                    _logger.Error(ex);
                    return false;
                }
            }
            else 
            {
                return false;
            }
        }
    }
}
