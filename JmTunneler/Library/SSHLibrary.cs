using NLog;
using Renci.SshNet;
using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JmTunneler.Library
{
    public class SSHLibrary
    {
        private static Logger logger = LogManager.GetLogger("JmTunneler_Log");
        SshClient sshClient = null;
        ForwardedPortRemote _rForwardPort = null;
        ForwardedPortLocal _lForwardPort = null;
        ConnectionInfo _connectionInfo = null;

        public SSHLibrary(ConnectionInfo _connectionInfo)
        {
            this._connectionInfo = _connectionInfo;

            sshClient = new SshClient(_connectionInfo);
            logger.Debug("Trying SSH connection...");
            sshClient.KeepAliveInterval = new TimeSpan(0, 0, 5); //5초마다 HeartBeat
            sshClient.Connect();
            logger.Debug("Connected SSH to {0}:{1}", _connectionInfo.Host, _connectionInfo.Port);
        }

        public void RemoteForwardedPort(string _localIP, uint _localPort, string _remoteIP, uint _remotePort)
        {
            _rForwardPort = new ForwardedPortRemote(_remoteIP, _remotePort, _localIP, _localPort);

            _rForwardPort.Exception += delegate (object sender1, ExceptionEventArgs e1)
            {
                logger.Debug(e1.Exception.ToString());
            };

            _rForwardPort.RequestReceived += delegate (object sender, PortForwardEventArgs e)
            {
                logger.Debug(e.OriginatorHost + ":" + e.OriginatorPort);
            };

            sshClient.AddForwardedPort(_rForwardPort);
            _rForwardPort.Start();

            logger.Info("Forwarding local address {0}:{1} to remote address {2}:{3}", _rForwardPort.HostAddress, _rForwardPort.Port, _rForwardPort.BoundHostAddress, _rForwardPort.BoundPort);
        }

        public void LocalForwardedPort(string _localIP, uint _localPort, string _remoteIP, uint _remotePort)
        {
            _lForwardPort = new ForwardedPortLocal(_localIP, _localPort, _remoteIP, _remotePort);

            _lForwardPort.Exception += delegate (object sender1, ExceptionEventArgs e1)

            {
                logger.Debug(e1.Exception.ToString());
            };

            _lForwardPort.RequestReceived += delegate (object sender, PortForwardEventArgs e)
            {
                logger.Debug(e.OriginatorHost + ":" + e.OriginatorPort);
            };

            sshClient.AddForwardedPort(_lForwardPort);
            _lForwardPort.Start();

            logger.Info("Forwarding remote address {0}:{1} to local address {2}:{3}", _lForwardPort.BoundHost, _lForwardPort.BoundPort, _lForwardPort.Host, _lForwardPort.Port);
        }

        public bool IsConnected()
        {
            return sshClient.IsConnected;
        }

        public void Dispose()
        {
            _lForwardPort.Dispose();
            sshClient.Disconnect();
            sshClient.Dispose();

            logger.Debug("Disconnected SSH to {0}:{1}", _connectionInfo.Host, _connectionInfo.Port);
        }
    }
}
