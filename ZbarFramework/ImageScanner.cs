using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static System.Net.Mime.MediaTypeNames;

namespace ZBar
{
    /// <summary>
    /// 中级图像扫描仪接口。从二维图像中读取条形码
    /// </summary>
    public class ImageScanner : IDisposable
    {
        private IntPtr handle = IntPtr.Zero;
        private bool cache = false;

        /// <summary>
        /// 创建 ImageScanner
        /// </summary>
        public ImageScanner()
        {
            this.handle = NativeZBar.zbar_image_scanner_create();
            if (this.handle == IntPtr.Zero)
                throw new Exception("未能创建基础image_scanner!");
        }

        #region Wrapper methods
        /// <summary>
        /// 扫描图像中的 symbols,一旦扫描了image，结果将与image相关联。使用image.Symbols访问symbol列表。
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        /// <exception cref="Exception">错误信息</exception>
        public int Scan(Image image)
        {
            int count = NativeZBar.zbar_scan_image(this.handle, image.Handle);
            if (count < 0)
                throw new Exception("图像扫描失败!");
            return count;
        }

        /// <summary>
        /// 扫描图像中的symbols
        /// </summary>
        /// <param name="image">
        /// 要扫描的 <see cref="System.Drawing.Image"/>文件
        /// </param>
        /// <remarks>
        /// 该方法将图像转换为适当的格式，并立即发布转换后的图像。同时将所有符号复制到列表中。
        /// </remarks>
        /// <returns>
        /// 图像中找到的symbol列表
        /// </returns>
        public List<Symbol> Scan(System.Drawing.Image image)
        {
            using (Image zimg = new Image(image))
            {
                using (Image grey = zimg.Convert(Image.FourCC('Y', '8', '0', '0')))
                {
                    this.Scan(grey);
                    return new List<Symbol>(grey.Symbols);
                }
            }
        }
        /// <value>
        /// 启用或禁用图像间结果缓存（默认禁用）。
        /// </value>
        /// <remarks>高速缓存主要用于扫描视频帧，同时为结果添加一些一致性检查和滞后性。此接口还会清除缓存。
        /// </remarks>
        public bool Cache
        {
            get
            {
                return this.cache;
            }
            set
            {
                NativeZBar.zbar_image_scanner_enable_cache(this.handle, value ? 1 : 0);
                this.cache = value;
            }
        }

        /// <summary>
        /// 将指示符号系统的配置（全部为 0）设置为指定值。
        /// </summary>
        public void SetConfiguration(SymbolType symbology, Config config, int value)
        {
            if (NativeZBar.zbar_image_scanner_set_config(this.handle, (int)symbology, (int)config, value) != 0)
                throw new Exception("Failed to set configuration");
        }

        #endregion Wrapper methods

        #region IDisposable Implementation

        //This pattern for implementing IDisposable is recommended by:
        //Framework Design Guidelines, 2. Edition, Section 9.4

        /// <summary>
        /// Dispose this object
        /// </summary>
        /// <remarks>
        /// This boolean disposing parameter here ensures that objects with a finalizer is not disposed,
        /// this is method is invoked from the finalizer. Do overwrite, and call, this method in base
        /// classes if you use any unmanaged resources.
        /// </remarks>
        /// <param name="disposing">
        /// A <see cref="System.Boolean"/> False if called from the finalizer, True if called from Dispose.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.handle != IntPtr.Zero)
            {
                NativeZBar.zbar_image_scanner_destroy(this.handle);
                this.handle = IntPtr.Zero;
            }
            if (disposing)
            {
                //Release finalizable resources, at the moment none.
            }
        }

        /// <summary>
        /// Release resources held by this object
        /// </summary>
        public void Dispose()
        {
            //We're disposing this object and can release objects that are finalizable
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalize this object
        /// </summary>
        ~ImageScanner()
        {
            //Dispose this object, but do NOT release finalizable objects, we don't know in which order
            //these are release and they may already be finalized.
            this.Dispose(false);
        }

        #endregion IDisposable Implementation

    }

    /// <summary>
    /// 图像扫描仪的配置参数
    /// </summary>
    public enum Config
    {
        /// <summary>
        /// 是否启用 symbology/feature
        /// </summary>
        Enable = 0,

        /// <summary>
        /// Enable check digit when optional
        /// </summary>
        AddCheck,

        /// <summary>
        /// Return check digit when present
        /// </summary>
        EmitCheck,

        /// <summary>
        /// Enable full ASCII character set
        /// </summary>
        ASCII,

        /// <summary>
        /// Number of boolean decoder configs
        /// </summary>
        Num,

        /// <summary>
        /// Minimum data length for valid decode
        /// </summary>
        MinimumLength = 0x20,

        /// <summary>
        /// Maximum data length for valid decode
        /// </summary>
        MaximumLength,

        /// <summary>
        /// Enable scanner to collect position data
        /// </summary>
        Position = 0x80,

        /// <summary>
        /// Image scanner vertical scan density
        /// </summary>
        XDensity = 0x100,

        /// <summary>
        /// Image scanner horizontical scan density
        /// </summary>
        YDensity
    }
}