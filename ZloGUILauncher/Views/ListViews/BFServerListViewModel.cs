using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Zlo.Extras;
using Zlo.Extras.BF_Servers;
using ZloGUILauncher.Servers;

namespace ZloGUILauncher.Views.ListViews
{
    public class BFServerListViewModel
    {
        public ObservableCollection<BF3_GUI_Server> BF3_GUI_Servers { get; } = new ObservableCollection<BF3_GUI_Server>();
        public ObservableCollection<BF4_GUI_Server> BF4_GUI_Servers { get; } = new ObservableCollection<BF4_GUI_Server>();
        public ObservableCollection<BFH_GUI_Server> BFH_GUI_Servers { get; } = new ObservableCollection<BFH_GUI_Server>();



        public Dictionary<ZloBFGame, DataGrid> DataGrids { get; } = new Dictionary<ZloBFGame, DataGrid>();
        IList GetGUIServerList(ZloBFGame game)
        {
            switch (game)
            {
                case ZloBFGame.BF_3:
                    return BF3_GUI_Servers;
                case ZloBFGame.BF_4:
                    return BF4_GUI_Servers;
                case ZloBFGame.BF_HardLine:
                    return BFH_GUI_Servers;
                case ZloBFGame.None:
                default:
                    return null;
            }
        }
        BF_GUI_Server CreateServer(ZloBFGame game, IBFServerBase baseServer)
        {
            switch (game)
            {
                case ZloBFGame.BF_3:
                    return new BF3_GUI_Server(baseServer);
                case ZloBFGame.BF_4:
                    return new BF4_GUI_Server(baseServer);
                case ZloBFGame.BF_HardLine:
                    return new BFH_GUI_Server(baseServer);
                case ZloBFGame.None:
                default:
                    return null;
            }
        }

        public BFServerListViewModel()
        {
            App.Client.BF3Servers.ServerChanged += BF3Servers_ServerChanged;
            App.Client.BF4Servers.ServerChanged += BF4Servers_ServerChanged;
            App.Client.BFHServers.ServerChanged += BFHServers_ServerChanged;           
        }

        private void BFHServers_ServerChanged(IBFServerList<API_BFHServerBase> list, uint id, API_BFHServerBase server, ServerChangeTypes changeType)
        {
            RespondToChange(ZloBFGame.BF_HardLine, list, server, changeType);
        }
        private void BF4Servers_ServerChanged(IBFServerList<API_BF4ServerBase> list, uint id, API_BF4ServerBase server, ServerChangeTypes changeType)
        {
            RespondToChange(ZloBFGame.BF_4, list, server, changeType);
        }
        private void BF3Servers_ServerChanged(IBFServerList<API_BF3ServerBase> list, uint id, API_BF3ServerBase server, ServerChangeTypes changeType)
        {
            RespondToChange(ZloBFGame.BF_3, list, server, changeType);
        }

        public static void AnimateRow(DataGrid dg, object element)
        {
            if (!(dg?.ItemContainerGenerator.ContainerFromItem(element) is DataGridRow row)) return;
            var switchOnAnimation = new ColorAnimation
            {
                From = Colors.White,
                To = Colors.Pink,
                Duration = TimeSpan.FromSeconds(1),
                AutoReverse = true
            };

            var blinkStoryboard = new Storyboard();


            blinkStoryboard.Children.Add(switchOnAnimation);
            Storyboard.SetTargetProperty(switchOnAnimation, new PropertyPath("Background.Color"));
            //animate changed server
            Storyboard.SetTarget(switchOnAnimation, row);

            row.BeginStoryboard(blinkStoryboard);
        }
        private void RespondToChange(ZloBFGame game, IBFServerList senderList, IBFServerBase serverBase, ServerChangeTypes changeType)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var guiList = GetGUIServerList(game);
              
                var equi = guiList.Cast<BF_GUI_Server>().FirstOrDefault(x => x.ID == serverBase.ServerID);
                switch (changeType)
                {
                    case ServerChangeTypes.Add:
                        if (equi == null)
                            guiList.Add(equi = CreateServer(game, serverBase));
                        AnimateRow(DataGrids[game], equi);
                        break;
                    case ServerChangeTypes.Update:
                        if (equi != null)
                        {
                            //notify the gui
                            equi.UpdateAllProps();
                            AnimateRow(DataGrids[game],equi);
                        }
                        break;
                    case ServerChangeTypes.Remove:
                        //remove from current list
                        if (equi != null)
                            guiList.Remove(equi);
                        break;
                    default:
                        break;
                }

            });
        }
    }
}
