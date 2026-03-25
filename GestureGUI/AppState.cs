using System.Collections.Generic;
using System.Linq;

namespace GestureGUI
{
    public class BluetoothUser
    {
        public string UserName { get; set; }
        public string UserType { get; set; }
        public string DeviceName { get; set; }
    }

    public static class AppState
    {
        public static string SelectedUserName { get; set; }
        public static string SelectedUserType { get; set; }
        public static string SelectedDeviceName { get; set; }

        public static List<BluetoothUser> BluetoothUsers { get; set; } = new List<BluetoothUser>();

        public static void AddBluetoothUser(string userName, string userType, string deviceName)
        {
            bool exists = BluetoothUsers.Any(u =>
                u.UserName == userName &&
                u.UserType == userType &&
                u.DeviceName == deviceName);

            if (!exists)
            {
                BluetoothUsers.Add(new BluetoothUser
                {
                    UserName = userName,
                    UserType = userType,
                    DeviceName = deviceName
                });
            }
        }

        public static void ClearBluetoothUsers()
        {
            BluetoothUsers.Clear();
        }
    }
}