using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Security;

namespace Watsys
{
    class InfoHandler
    {
        private FirebaseRest fbr;

        public InfoHandler(FirebaseRest firebaseRest)
        {
            fbr = firebaseRest;
        }


        public void Handle(InfoPN info)
        {
            //Debug.WriteLine(string.Format("\n0: {0}, 1: {1}, 2: {2}, 3: {3}, 4: {4}, 5: {5}",info.User, info.Group,info.ShutDown,info.CaptureGroup,info.OpenGroup,info.CloseAll));

            if (fbr == null || info.User == "" || info.Group == "")
            {
                Debug.WriteLine(fbr == null ? "The FirebaseRest object is not initialized"
                    : "The User or Group fields are empty");
                return;
            }

            if (info.CaptureGroup > 0) fbr.CaptureGroup(info.Group, info.User);
            

            if (info.OpenGroup > 0) fbr.OpenGroup(info.Group, info.User);
            

            if (info.CloseAll > 0) Patherian.CloseAllProcesses();

            if (info.ShutDown > 0) Patherian.ShutDown();


        }
        
    }
}
