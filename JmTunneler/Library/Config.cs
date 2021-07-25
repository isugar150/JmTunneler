using System;
using System.IO;
using System.Reflection;

namespace JmTunneler.Library
{
    internal interface iniproperties
    {
        string Lock_Password { get; set; }
        string Auto_Reconnect { get; set; }

        string SSH_IP { get; set; }
        string SSH_ID { get; set; }
        string SSH_PW { get; set; }

        string Type { get; set; }
        string List_Interface { get; set; }
        string List_Port { get; set; }
        string Dest_Host { get; set; }
        string Dest_Port { get; set; }
    }

    public class iniProperties : iniproperties
    {
        private string _Lock_Password = "0000";
        private string _Auto_Reconnect = "Y";

        private string _SSH_IP = "";
        private string _SSH_ID = "";
        private string _SSH_PW = "";

        private string _Type = "0";
        private string _List_Interface = "";
        private string _List_Port = "";
        private string _Dest_Host = "";
        private string _Dest_Port = "";

        public string Lock_Password { get { return _Lock_Password; } set { _Lock_Password = value; } }
        public string Auto_Reconnect { get { return _Auto_Reconnect; } set { _Auto_Reconnect = value; } }

        public string SSH_IP { get { return _SSH_IP; } set { _SSH_IP = value; } }
        public string SSH_ID { get { return _SSH_ID; } set { _SSH_ID = value; } }
        public string SSH_PW { get { return _SSH_PW; } set { _SSH_PW = value; } }

        public string Type { get { return _Type; } set { _Type = value; } }
        public string List_Interface { get { return _List_Interface; } set { _List_Interface = value; } }
        public string List_Port { get { return _List_Port; } set { _List_Port = value; } }
        public string Dest_Host { get { return _Dest_Host; } set { _Dest_Host = value; } }
        public string Dest_Port { get { return _Dest_Port; } set { _Dest_Port = value; } }
    }

    public class Setting
    {
        public static void createSetting()
        {
            IniFile setting = new IniFile();

            setting["Common"]["SSH IP"] = "";
            setting["Common"]["SSH ID"] = "";
            setting["Common"]["SSH PW"] = "";

            setting["Common"]["Type"] = "S2C";
            setting["Common"]["List Interface"] = "";
            setting["Common"]["List Port"] = "";
            setting["Common"]["Dest Host"] = "";
            setting["Common"]["Dest Port"] = "";

            setting.Save(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName + @".\Setting.ini");
        }
    }
}
