using GoLib;
using GoLib.SGF;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;

namespace Kifu.Utils
{
    public static class SgfHelper
    {
        public static string ToString(Goban goban)
        {
            return SgfWriter.ToSGF(goban);
        }

        public static Goban FromString(string sgf)
        {
            return SgfParser.SgfDecode(sgf);
        }

        public async static Task<string> Open()
        {
            var openPicker = new FileOpenPicker();
            openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            //openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.FileTypeFilter.Clear();
            openPicker.FileTypeFilter.Add(".sgf");

            var file = await openPicker.PickSingleFileAsync();

            if (file != null)
            {
                var buffer = await FileIO.ReadBufferAsync(file);
                using (var reader = DataReader.FromBuffer(buffer))
                {
                    return reader.ReadString(buffer.Length);
                }

                //this.DataContext = file;
                //var mruToken = StorageApplicationPermissions.MostRecentlyUsedList.Add(file);
            }

            return null;
        }

        public async static Task Save(string content)
        {
            var savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("Smart Game Format", new List<string>() { ".sgf" });
            savePicker.DefaultFileExtension = ".sgf";
            savePicker.SuggestedFileName = "Game " + DateTime.Now.ToString("u");
            var file = await savePicker.PickSaveFileAsync();

            if (null != file) // file is null if user cancels the file picker.
            {
                using (var writeStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    writeStream.Size = 0;
                    using (var oStream = writeStream.GetOutputStreamAt(0))
                    {
                        using (var dWriter = new DataWriter(oStream))
                        {
                            dWriter.WriteString(content);
                            await dWriter.StoreAsync();
                            await oStream.FlushAsync();
                        }
                    }
                }
                //this.DataContext = file;
                //var mruToken = StorageApplicationPermissions.MostRecentlyUsedList.Add(file);
            }
        }
    }
}
