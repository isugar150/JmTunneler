using JmTunneler.Library;
using NLog;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JmTunneler
{
    public partial class Service1 : ServiceBase
    {
        private static Logger logger = LogManager.GetLogger("JmTunneler_Log");
        int currentProcess = 0;
        private static iniProperties IniProperties = new iniProperties(); // Ini 설정값 변수
        private static SSHLibrary ssh = null;
        private static ConnectionInfo info = null;
        private static bool autoReConnect = true;
        private static Thread t1 = null;

        public Service1()
        {
            InitializeComponent();
            currentProcess = int.Parse(Process.GetCurrentProcess().Id.ToString());
        }

        protected override void OnStart(string[] args)
        {
            #region parse Ini File
            FileInfo settingFile = new FileInfo(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName + @".\Setting.ini");
            logger.Info("Parsing ini File :: " + settingFile.FullName);
            try
            {
                IniFile pairs = new IniFile(); // Ini 변수 초기화
                if (!settingFile.Exists)
                {
                    Setting.createSetting();
                }
                pairs.Load(settingFile.FullName);
                try
                {
                    IniProperties.SSH_IP = pairs["Common"]["SSH IP"].ToString().Trim();
                    IniProperties.SSH_ID = pairs["Common"]["SSH ID"].ToString().Trim();
                    IniProperties.SSH_PW = pairs["Common"]["SSH PW"].ToString().Trim();

                    IniProperties.Type = pairs["Common"]["Type"].ToString().Trim();
                    IniProperties.List_Interface = pairs["Common"]["List Interface"].ToString().Trim();
                    IniProperties.List_Port = pairs["Common"]["List Port"].ToString().Trim();
                    IniProperties.Dest_Host = pairs["Common"]["Dest Host"].ToString().Trim();
                    IniProperties.Dest_Port = pairs["Common"]["Dest Port"].ToString().Trim();
                }
                catch (Exception e1)
                {
                    logger.Error(e1.Message);
                    logger.Error(e1.StackTrace);
                    logger.Error("Delete the Setting.ini file in the path and try again.");
                    Process.GetProcessById(currentProcess).Kill();
                }

                logger.Info("ini file parsed");
            }
            catch (NullReferenceException e1)
            {
                logger.Error(e1.Message);
                logger.Error(e1.StackTrace);
                logger.Error("There is a problem with the config file and continues with default settings");
                Process.GetProcessById(currentProcess).Kill();
            }
            catch (Exception e1)
            {
                logger.Error(e1.Message);
                logger.Error(e1.StackTrace);
                logger.Error("There is a problem with the config file and continues with default settings");
                Process.GetProcessById(currentProcess).Kill();
            }
            #endregion

            #region Start Tunneling
            try
            {
                string sshIp = "";
                int sshPort = 0;
                if (IniProperties.SSH_IP.Contains(":"))
                {
                    sshIp = IniProperties.SSH_IP.Split(':')[0];
                    sshPort = int.Parse(IniProperties.SSH_IP.Split(':')[1]);
                }
                else
                {
                    sshIp = IniProperties.SSH_IP;
                    sshPort = 22;
                }

                logger.Info("myPid:          " + currentProcess);
                logger.Info("sshIp:          " + sshIp);
                logger.Info("sshPort:        " + sshPort);
                logger.Info("Type:           " + IniProperties.Type);
                logger.Info("List_Interface: " + IniProperties.List_Interface);
                logger.Info("List_Port:      " + IniProperties.List_Port);
                logger.Info("Dest_Host:      " + IniProperties.Dest_Host);
                logger.Info("Dest_Port:      " + IniProperties.Dest_Port);

                PasswordAuthenticationMethod password = new PasswordAuthenticationMethod(IniProperties.SSH_ID, IniProperties.SSH_PW);
                info = new ConnectionInfo(sshIp, sshPort, IniProperties.SSH_ID, password);
            } catch (Exception e1)
            {
                logger.Error(e1.Message);
                logger.Error(e1.StackTrace);
                Process.GetProcessById(currentProcess).Kill();
            }
            #endregion

            #region Background Logic
            t1 = new Thread(delegate ()
            {
                while (autoReConnect)
                {
                    try
                    {

                        if(ssh == null)
                        {
                            ssh = new SSHLibrary(info);
                            logger.Info("You have logged in to the target SSH server with the account {}.", info.Username);
                        }

                        IEnumerable<ForwardedPort> forwardedPorts = ssh.getForwardedPorts();

                        if (forwardedPorts.ToArray().Length == 0)
                        {
                            if (IniProperties.Type.Equals("C2S"))
                            {
                                ssh.LocalForwardedPort(IniProperties.Dest_Host, uint.Parse(IniProperties.Dest_Port), IniProperties.List_Interface, uint.Parse(IniProperties.List_Port));
                            }
                            else if (IniProperties.Type.Equals("S2C"))
                            {
                                ssh.RemoteForwardedPort(IniProperties.Dest_Host, uint.Parse(IniProperties.Dest_Port), IniProperties.List_Interface, uint.Parse(IniProperties.List_Port));
                            }
                        }
                        Thread.Sleep(5000);
                    }
                    catch (SocketException) { logger.Debug("Socket is Close"); }
                    catch (Exception e1)
                    {
                        autoReConnect = false;
                        logger.Error(e1);
                        Process.GetProcessById(currentProcess).Kill();
                    }
                }
            });
            t1.Start();
            #endregion
        }

        protected override void OnStop()
        {
            ssh.Dispose();
            Process.GetProcessById(currentProcess).Kill();
        }
    }
}
