using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace ME3Server_WV
{
    public partial class GUI_PacketEditor : Window
    {
        public List<Blaze.Packet> Packets;
        public List<Blaze.Tdf> inlist;
        public int inlistcount;
        public int lastsearchtype = -1;
        public int lastsearch;
        private ObservableCollection<TreeItemModel> treeItems = new();

        public GUI_PacketEditor()
        {
            InitializeComponent();
            treeView1.ItemsSource = treeItems;
        }

        private async void openBINMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open BIN",
                FileTypeFilter = new[] { new FilePickerFileType("BIN files") { Patterns = new[] { "*.bin" } } },
                AllowMultiple = false
            });

            if (files.Count == 0) return;
            try
            {
                var filePath = files[0].TryGetLocalPath();
                if (filePath == null) return;
                MemoryStream m = new MemoryStream(File.ReadAllBytes(filePath));
                Packets = Blaze.FetchAllBlazePackets(m);
                RefreshStuff();
                this.Title = "Packet Viewer - " + Path.GetFileName(filePath);
            }
            catch (Exception ex)
            {
                Logger.Log("Packet Editor Error: " + ex.Message, LogColor.Red);
                Packets = null;
            }
        }

        public void RefreshStuff()
        {
            if (Packets == null)
                return;
            var items = new List<string>();
            int count = 0;
            foreach (Blaze.Packet p in Packets)
            {
                string s = (count++).ToString() + " : ";
                s += p.Length.ToString("X4") + " ";
                s += p.Component.ToString("X4") + " ";
                s += p.Command.ToString("X4") + " ";
                s += p.Error.ToString("X4") + " ";
                s += p.QType.ToString("X4") + " ";
                s += p.ID.ToString("X4") + " ";
                s += p.extLength.ToString("X4") + " ";
                byte qtype = (byte)(p.QType >> 8);
                switch (qtype)
                {
                    case 0:
                        s += "[Client]";
                        break;
                    case 0x10:
                        s += "[Server]";
                        break;
                    case 0x20:
                        s += "[Server][Async]";
                        break;
                    case 0x30:
                        s += "[Server][Error]";
                        break;
                }
                s += "[INFO] " + Blaze.PacketToDescriber(p);
                items.Add(s);
            }
            listBox1.ItemsSource = items;
        }

        private void listBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1) return;
            try
            {
                rtb2.Text = Blaze.HexDump(Blaze.PacketToRaw(Packets[n]));
                treeItems.Clear();
                rtb1.Text = "";
                inlist = new List<Blaze.Tdf>();
                inlistcount = 0;
                List<Blaze.Tdf> Fields = Blaze.ReadPacketContent(Packets[n]);
                foreach (Blaze.Tdf tdf in Fields)
                    treeItems.Add(TdfToTree(tdf));
            }
            catch (Exception ex)
            {
                rtb1.Text = "Error:\n" + ex.Message;
            }
        }

        private TreeItemModel TdfToTree(Blaze.Tdf tdf)
        {
            TreeItemModel t, t2, t3;
            switch (tdf.Type)
            {
                case 3:
                    t = tdf.ToTree();
                    Blaze.TdfStruct str = (Blaze.TdfStruct)tdf;
                    if (str.startswith2)
                        t.Text += " (Starts with 2)";
                    foreach (Blaze.Tdf td in str.Values)
                        t.Children.Add(TdfToTree(td));
                    t.Name = (inlistcount++).ToString();
                    inlist.Add(tdf);
                    return t;
                case 4:
                    t = tdf.ToTree();
                    Blaze.TdfList l = (Blaze.TdfList)tdf;
                    if (l.SubType == 3)
                    {
                        List<Blaze.TdfStruct> l2 = (List<Blaze.TdfStruct>)l.List;
                        for (int i = 0; i < l2.Count; i++)
                        {
                            t2 = new TreeItemModel("Entry #" + i);
                            if (l2[i].startswith2)
                                t2.Text += " (Starts with 2)";
                            List<Blaze.Tdf> l3 = l2[i].Values;
                            for (int j = 0; j < l3.Count; j++)
                                t2.Children.Add(TdfToTree(l3[j]));
                            t.Children.Add(t2);
                        }
                    }
                    t.Name = (inlistcount++).ToString();
                    inlist.Add(tdf);
                    return t;
                case 5:
                    t = tdf.ToTree();
                    Blaze.TdfDoubleList ll = (Blaze.TdfDoubleList)tdf;
                    t2 = new TreeItemModel("List 1");
                    if (ll.SubType1 == 3)
                    {
                        List<Blaze.TdfStruct> l2 = (List<Blaze.TdfStruct>)ll.List1;
                        for (int i = 0; i < l2.Count; i++)
                        {
                            t3 = new TreeItemModel("Entry #" + i);
                            if (l2[i].startswith2)
                                t2.Text += " (Starts with 2)";
                            List<Blaze.Tdf> l3 = l2[i].Values;
                            for (int j = 0; j < l3.Count; j++)
                                t3.Children.Add(TdfToTree(l3[j]));
                            t2.Children.Add(t3);
                        }
                        t.Children.Add(t2);
                    }
                    t2 = new TreeItemModel("List 2");
                    if (ll.SubType2 == 3)
                    {
                        List<Blaze.TdfStruct> l2 = (List<Blaze.TdfStruct>)ll.List2;
                        for (int i = 0; i < l2.Count; i++)
                        {
                            t3 = new TreeItemModel("Entry #" + i);
                            if (l2[i].startswith2)
                                t2.Text += " (Starts with 2)";
                            List<Blaze.Tdf> l3 = l2[i].Values;
                            for (int j = 0; j < l3.Count; j++)
                                t3.Children.Add(TdfToTree(l3[j]));
                            t2.Children.Add(t3);
                        }
                        t.Children.Add(t2);
                    }
                    t.Name = (inlistcount++).ToString();
                    inlist.Add(tdf);
                    return t;
                case 6:
                    t = tdf.ToTree();
                    Blaze.TdfUnion tu = (Blaze.TdfUnion)tdf;
                    if (tu.UnionType != 0x7F)
                    {
                        t.Children.Add(TdfToTree(tu.UnionContent));
                    }
                    t.Name = (inlistcount++).ToString();
                    inlist.Add(tdf);
                    return t;
                default:
                    t = tdf.ToTree();
                    t.Name = (inlistcount++).ToString();
                    inlist.Add(tdf);
                    return t;
            }
        }

        private void treeView1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (treeView1.SelectedItem is TreeItemModel t && t.Name != "")
            {
                int n = Convert.ToInt32(t.Name);
                Blaze.Tdf tdf = inlist[n];
                string s;
                switch (tdf.Type)
                {
                    case 0:
                        Blaze.TdfInteger ti = (Blaze.TdfInteger)tdf;
                        rtb1.Text = "0x" + ti.Value.ToString("X");
                        if (ti.Label == "IP  ")
                        {
                            rtb1.Text += Environment.NewLine + "(" + ME3Server.GetStringFromIP(ti.Value) + ")";
                        }
                        break;
                    case 1:
                        rtb1.Text = ((Blaze.TdfString)tdf).Value.ToString();
                        break;
                    case 2:
                        rtb1.Text = "Length: " + ((Blaze.TdfBlob)tdf).Data.Length.ToString();
                        rtb1.Text += Environment.NewLine + Blaze.HexDump(((Blaze.TdfBlob)tdf).Data);
                        break;
                    case 4:
                        Blaze.TdfList l = (Blaze.TdfList)tdf;
                        s = "";
                        for (int i = 0; i < l.Count; i++)
                        {
                            switch (l.SubType)
                            {
                                case 0:
                                    s += "{" + ((List<long>)l.List)[i] + "} ";
                                    break;
                                case 1:
                                    s += "{" + ((List<string>)l.List)[i] + "} ";
                                    break;
                                case 9:
                                    Blaze.TrippleVal tv = ((List<Blaze.TrippleVal>)l.List)[i];
                                    s += "{" + tv.v1.ToString("X") + "; " + tv.v2.ToString("X") + "; " + tv.v3.ToString("X") + "} ";
                                    break;
                            }
                        }
                        rtb1.Text = s;
                        break;
                    case 5:
                        s = "";
                        Blaze.TdfDoubleList dll = (Blaze.TdfDoubleList)tdf;
                        for (int i = 0; i < dll.Count; i++)
                        {
                            s += "{";
                            switch (dll.SubType1)
                            {
                                case 0:
                                    List<long> l1 = (List<long>)dll.List1;
                                    s += l1[i].ToString("X");
                                    break;
                                case 1:
                                    List<string> l2s = (List<string>)dll.List1;
                                    s += l2s[i];
                                    break;
                                case 0xA:
                                    List<float> lf1 = (List<float>)dll.List1;
                                    s += lf1[i].ToString();
                                    break;
                                default:
                                    s += "(see List1[" + i + "])";
                                    break;
                            }
                            s += " ; ";
                            switch (dll.SubType2)
                            {
                                case 0:
                                    List<long> l1 = (List<long>)dll.List2;
                                    s += l1[i].ToString("X");
                                    break;
                                case 1:
                                    List<string> l2s = (List<string>)dll.List2;
                                    s += l2s[i];
                                    break;
                                case 0xA:
                                    List<float> lf1 = (List<float>)dll.List2;
                                    s += lf1[i].ToString();
                                    break;
                                default:
                                    s += "(see List2[" + i + "])";
                                    break;
                            }
                            s += "}\n";
                        }
                        rtb1.Text = s;
                        break;
                    case 6:
                        rtb1.Text = "Type: 0x" + ((Blaze.TdfUnion)tdf).UnionType.ToString("X2");
                        break;
                    case 7:
                        Blaze.TdfIntegerList til = (Blaze.TdfIntegerList)tdf;
                        s = "";
                        for (int i = 0; i < til.Count; i++)
                        {
                            s += til.List[i].ToString("X");
                            if (i < til.Count - 1)
                                s += "; ";
                        }
                        rtb1.Text = s;
                        break;
                    case 8:
                        Blaze.TdfDoubleVal dval = (Blaze.TdfDoubleVal)tdf;
                        rtb1.Text = "0x" + dval.Value.v1.ToString("X") + " 0x" + dval.Value.v2.ToString("X");
                        break;
                    case 9:
                        Blaze.TdfTrippleVal tval = (Blaze.TdfTrippleVal)tdf;
                        rtb1.Text = "0x" + tval.Value.v1.ToString("X") + " 0x" + tval.Value.v2.ToString("X") + " 0x" + tval.Value.v3.ToString("X");
                        break;
                    default:
                        rtb1.Text = "";
                        break;
                }
            }
        }

        private void toolStripButton1_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            string s = toolStripTextBox1.Text.Replace(" ", "");
            if (s.Length != 6)
                return;
            string v = s + "00";
            List<byte> tmp = new List<byte>(Blaze.StringToByteArray(v));
            tmp.Reverse();
            uint val = BitConverter.ToUInt32(tmp.ToArray(), 0);
            string label = Blaze.TagToLabel(val);
            toolStripTextBox2.Text = label;
        }

        private async void saveRawMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1) return;
            var topLevel = TopLevel.GetTopLevel(this);
            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save Raw",
                DefaultExtension = "bin",
                FileTypeChoices = new[] { new FilePickerFileType("BIN files") { Patterns = new[] { "*.bin" } } }
            });
            if (file != null)
            {
                var filePath = file.TryGetLocalPath();
                if (filePath != null)
                {
                    File.WriteAllBytes(filePath, Blaze.PacketToRaw(Packets[n]));
                    Logger.Log("Raw packet saved.", LogColor.Black);
                }
            }
        }

        private void toolStripButton2_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            byte[] buff = Blaze.Label2Tag(toolStripTextBox2.Text);
            string s = "";
            for (int i = 0; i < 3; i++)
                s += buff[i].ToString("X2") + " ";
            toolStripTextBox1.Text = s;
        }

        private async void exportTreeMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (treeItems.Count == 0)
                return;
            var topLevel = TopLevel.GetTopLevel(this);
            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Export Tree",
                DefaultExtension = "txt",
                FileTypeChoices = new[] { new FilePickerFileType("Text files") { Patterns = new[] { "*.txt" } } }
            });
            if (file != null)
            {
                var filePath = file.TryGetLocalPath();
                if (filePath != null)
                {
                    File.WriteAllText(filePath, ReadNodes(0, treeItems));
                    Logger.Log("Tree exported.", LogColor.Black);
                }
            }
        }

        private string ReadNodes(int tab, IEnumerable<TreeItemModel> nodes)
        {
            string tb = "";
            for (int i = 0; i < tab; i++)
                tb += "\t";
            string res = "";
            foreach (var node in nodes)
            {
                res += tb + node.Text + "\r\n";
                if (node.Children.Count != 0)
                    res += ReadNodes(tab + 1, node.Children);
            }
            return res;
        }

        private void toolStripButton3_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            string s = toolStripTextBox3.Text.Replace(" ", "");
            if (s == "")
                return;
            long l = Convert.ToInt64(s, 16);
            MemoryStream m = new MemoryStream();
            Blaze.CompressInteger(l, m);
            m.Seek(0, 0);
            string r = "";
            for (int i = 0; i < m.Length; i++)
                r += ((byte)m.ReadByte()).ToString("X2") + " ";
            toolStripTextBox4.Text = r;
        }

        private void toolStripButton4_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            string s = toolStripTextBox4.Text.Replace(" ", "");
            if (s == "")
                return;
            MemoryStream m = new MemoryStream();
            for (int i = 0; i < s.Length / 2; i++)
                m.WriteByte(Convert.ToByte(s.Substring(i * 2, 2), 16));
            m.Seek(0, 0);
            long l = Blaze.DecompressInteger(m);
            toolStripTextBox3.Text = l.ToString("X");
        }

        private void treeMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            treeView1.IsVisible = true;
            rtb2.IsVisible = false;
        }

        private void rawMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            treeView1.IsVisible = false;
            rtb2.IsVisible = true;
        }

        private async void findByComponentMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (listBox1.ItemCount == 0) return;
            var dialog = new InputDialog("Find by Component", "Please Enter ID in hex", "9");
            string result = await dialog.ShowDialog<string>(this);
            if (string.IsNullOrEmpty(result)) return;
            int ID = Convert.ToInt32(result, 16);
            int n = listBox1.SelectedIndex;
            for (int i = n + 1; i < listBox1.ItemCount; i++)
                if (Packets[i].Component == ID)
                {
                    listBox1.SelectedIndex = i;
                    lastsearch = ID;
                    lastsearchtype = 0;
                    break;
                }
        }

        private void searchAgainMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (lastsearchtype == -1) return;
            if (listBox1.ItemCount == 0) return;
            int n = listBox1.SelectedIndex;
            for (int i = n + 1; i < listBox1.ItemCount; i++)
            {
                switch (lastsearchtype)
                {
                    case 0:
                        if (Packets[i].Component == lastsearch)
                        {
                            listBox1.SelectedIndex = i;
                            return;
                        }
                        break;
                    case 1:
                        if (Packets[i].Command == lastsearch)
                        {
                            listBox1.SelectedIndex = i;
                            return;
                        }
                        break;
                }
            }
        }

        private async void findByCommandMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (listBox1.ItemCount == 0) return;
            var dialog = new InputDialog("Find by Command", "Please Enter ID in hex", "1D");
            string result = await dialog.ShowDialog<string>(this);
            if (string.IsNullOrEmpty(result)) return;
            int ID = Convert.ToInt32(result, 16);
            int n = listBox1.SelectedIndex;
            for (int i = n + 1; i < listBox1.ItemCount; i++)
                if (Packets[i].Command == ID)
                {
                    listBox1.SelectedIndex = i;
                    lastsearch = ID;
                    lastsearchtype = 1;
                    break;
                }
        }
    }
}
