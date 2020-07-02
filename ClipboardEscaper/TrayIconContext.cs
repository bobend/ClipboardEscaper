using System;
using System.Reflection;
using System.Windows.Forms;

namespace ClipboardEscaper
{
    internal class TrayIconContext : ApplicationContext
    {
        private readonly NotifyIcon _trayIcon;
        private Assembly _assembly => typeof(TrayIconContext).GetTypeInfo().Assembly;
        private readonly KeyboardHook _escapeHook, _unescapeHook;

        public TrayIconContext(string[] args)
        {
            _escapeHook = new KeyboardHook();
            _escapeHook.RegisterHotKey(ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Shift, Keys.E);
            _escapeHook.KeyPressed += Escape;           
            _unescapeHook = new KeyboardHook();
            _unescapeHook.RegisterHotKey(ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Shift, Keys.U);
            _unescapeHook.KeyPressed += Unescape;
            var contextMenuStrip = new ContextMenuStrip();
            contextMenuStrip.Items.AddRange(new[] {
                    new ToolStripMenuItem("&Escape (Ctrl+Alt+Shift+E)", null, Escape),
                    new ToolStripMenuItem("&Unescape (Ctrl+Alt+Shift+U)", null, Unescape),
                    new ToolStripMenuItem("&About", null, About),
                    new ToolStripMenuItem("E&xit", null, Exit),
                });

            _trayIcon = new NotifyIcon()
            {
                Icon = new System.Drawing.Icon(_assembly.GetManifestResourceStream("ClipboardEscaper.Icon.ico") ?? throw new InvalidOperationException()),
                ContextMenuStrip = contextMenuStrip,
                Visible = true,
                Text = "Clipboard Escaper",
            };
        }

        void Escape(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
                Clipboard.SetText(Clipboard.GetText().Replace(@"\", @"\\"));
        }

        void Unescape(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
                Clipboard.SetText(Clipboard.GetText().Replace(@"\\", @"\"));
        }

        void Exit(object sender, EventArgs e)
        {
            _unescapeHook?.Dispose();
            _escapeHook?.Dispose();
            // Hide tray icon, otherwise it will remain shown until user mouses over it
            _trayIcon.Visible = false;
            Application.Exit();
        }

        void About(object sender, EventArgs e)
        {
            var aboutText = string.Empty;
            var aboutCaption = string.Empty;

            if (Attribute.IsDefined(_assembly, typeof(AssemblyDescriptionAttribute)))
            {
                var a = (AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(_assembly, typeof(AssemblyDescriptionAttribute));
                aboutText = a.Description;
            }

            if (Attribute.IsDefined(_assembly, typeof(AssemblyTitleAttribute)))
            {
                var a = (AssemblyTitleAttribute)Attribute.GetCustomAttribute(_assembly, typeof(AssemblyTitleAttribute));
                aboutCaption = a.Title;
            }

            MessageBox.Show(aboutText, aboutCaption, MessageBoxButtons.OK);
        }
    }
}
