using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;

namespace Ball_of_Duty_Server.Domain
{
    [Serializable]
    public abstract class ServerGameObject
    {

        protected ServerBody body;
        private int id;

        public ServerGameObject(int id)
        {
            this.id = id;
        }

       

        public int getID()
        {
            return id;
        }

        public void destroy()
        {

        }

        public ServerBody getBody()
        {
            return body;
        }

        public void setBody(ServerBody body)
        {
            this.body = body;
        }

    }
}