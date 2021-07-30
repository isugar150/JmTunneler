﻿using JmTunneler.Library;
using NLog;
using Renci.SshNet;
using Renci.SshNet.Common;
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
        private static Logger _logger = LogManager.GetLogger("JmTunneler_Log");
        private static iniProperties _IniProperties = new iniProperties(); // Ini 설정값 변수
        private static SSHLibrary _ssh = null;
        private static ConnectionInfo _info = null;
        private static ForwardedPort[] _forwardedPortList = null;
        private bool _firstLoop = true;
        private Timer workTimer;

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            #region parse Ini File
            FileInfo settingFile = new FileInfo(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName + @".\Setting.ini");
            _logger.Info("Parsing ini File :: " + settingFile.FullName);
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
                    _IniProperties.SSH_IP = pairs["Common"]["SSH IP"].ToString().Trim();
                    _IniProperties.SSH_ID = pairs["Common"]["SSH ID"].ToString().Trim();
                    _IniProperties.SSH_PW = pairs["Common"]["SSH PW"].ToString().Trim();

                    _IniProperties.Type = pairs["Common"]["Type"].ToString().Trim();
                    _IniProperties.List_Interface = pairs["Common"]["List Interface"].ToString().Trim();
                    _IniProperties.List_Port = pairs["Common"]["List Port"].ToString().Trim();
                    _IniProperties.Dest_Host = pairs["Common"]["Dest Host"].ToString().Trim();
                    _IniProperties.Dest_Port = pairs["Common"]["Dest Port"].ToString().Trim();
                }
                catch (Exception e1)
                {
                    _logger.Error(e1.Message);
                    _logger.Error(e1.StackTrace);
                    _logger.Error("Delete the Setting.ini file in the path and try again.");
                    base.OnStop();
                }

                _logger.Info("ini file parsed");
            }
            catch (NullReferenceException e1)
            {
                _logger.Error(e1.Message);
                _logger.Error(e1.StackTrace);
                _logger.Error("There is a problem with the config file and continues with default settings");
                base.OnStop();
            }
            catch (Exception e1)
            {
                _logger.Error(e1.Message);
                _logger.Error(e1.StackTrace);
                _logger.Error("There is a problem with the config file and continues with default settings");
                base.OnStop();
            }
            #endregion

            #region Start Tunneling
            try
            {
                string sshIp = "";
                int sshPort = 0;
                if (_IniProperties.SSH_IP.Contains(":"))
                {
                    sshIp = _IniProperties.SSH_IP.Split(':')[0];
                    sshPort = int.Parse(_IniProperties.SSH_IP.Split(':')[1]);
                }
                else
                {
                    sshIp = _IniProperties.SSH_IP;
                    sshPort = 22;
                }

                _logger.Info("sshIp:          " + sshIp);
                _logger.Info("sshPort:        " + sshPort);
                _logger.Info("Type:           " + _IniProperties.Type);
                _logger.Info("List_Interface: " + _IniProperties.List_Interface);
                _logger.Info("List_Port:      " + _IniProperties.List_Port);
                _logger.Info("Dest_Host:      " + _IniProperties.Dest_Host);
                _logger.Info("Dest_Port:      " + _IniProperties.Dest_Port);

                PasswordAuthenticationMethod password = new PasswordAuthenticationMethod(_IniProperties.SSH_ID, _IniProperties.SSH_PW);
                _info = new ConnectionInfo(sshIp, sshPort, _IniProperties.SSH_ID, password);
            } catch (Exception e1)
            {
                _logger.Error(e1.Message);
                _logger.Error(e1.StackTrace);
                base.OnStop();
            }
            #endregion

            #region Background Logic
            workTimer = new Timer(new TimerCallback(DoWork), null, 5000, 5000);
            //base.OnStart(args);
            #endregion
        }

        protected override void OnStop()
        {
            if(workTimer != null)
                workTimer.Dispose();
            if(_ssh != null)
            {
                _ssh.Dispose();
                _ssh = null;
            }
        }

        private void DoWork(object state)
        {
            try
            {
                _logger.Debug("in the Loop");
                if (_ssh == null)
                {
                    _logger.Debug("init ssh Start");
                    _ssh = new SSHLibrary(_info);
                    _logger.Info("You have logged in to the target SSH server with the account {}.", _info.Username);
                }

                _logger.Debug("ssh.getForwardedPorts().ToArray().Length: " + _ssh.getForwardedPorts().ToArray().Length);

                _logger.Debug("_IniProperties.Type is " + _IniProperties.Type);

                if (!_firstLoop)
                {
                    _forwardedPortList = _ssh.getForwardedPorts().ToArray();

                    _logger.Debug("forwardedPortList[{}].IsStarted: {}", 0, _forwardedPortList[0].IsStarted);

                    if (!_forwardedPortList[0].IsStarted)
                    {
                        if (_IniProperties.Type.Equals("C2S"))
                        {
                            _ssh.LocalForwardedPort(_IniProperties.Dest_Host, uint.Parse(_IniProperties.Dest_Port), _IniProperties.List_Interface, uint.Parse(_IniProperties.List_Port));
                        }
                        else if (_IniProperties.Type.Equals("S2C"))
                        {
                            _ssh.RemoteForwardedPort(_IniProperties.Dest_Host, uint.Parse(_IniProperties.Dest_Port), _IniProperties.List_Interface, uint.Parse(_IniProperties.List_Port));
                        }
                    }
                }
                else
                {
                    if (_IniProperties.Type.Equals("C2S"))
                    {
                        _ssh.LocalForwardedPort(_IniProperties.Dest_Host, uint.Parse(_IniProperties.Dest_Port), _IniProperties.List_Interface, uint.Parse(_IniProperties.List_Port));
                    }
                    else if (_IniProperties.Type.Equals("S2C"))
                    {
                        _ssh.RemoteForwardedPort(_IniProperties.Dest_Host, uint.Parse(_IniProperties.Dest_Port), _IniProperties.List_Interface, uint.Parse(_IniProperties.List_Port));
                    }
                }

                _firstLoop = false;
            }
            catch (SocketException)
            {
                if (_ssh != null)
                {
                    _ssh.Dispose();
                    _ssh = null;
                    _firstLoop = true;
                }
                _logger.Debug("Socket is Close");
            }
            catch (SshConnectionException e1)
            {
                if (_ssh != null)
                {
                    _ssh.Dispose();
                    _ssh = null;
                    _firstLoop = true;
                }
                _logger.Error(e1);
            }
            catch (SshException e1)
            {
                _logger.Info("List is already bound or Dest is already bound.");
                if (_ssh != null)
                {
                    _ssh.Dispose();
                    _ssh = null;
                    _firstLoop = true;
                }
                _logger.Debug(e1);
                base.OnStop();
            }
            catch (Exception e1)
            {
                _logger.Error(e1);
                base.OnStop();
            }
        }
    }
}
