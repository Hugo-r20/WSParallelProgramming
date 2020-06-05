using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WSParallelProgramming
{
    class Question1
    {
        delegate void DELG();
        delegate void EVT(object o);
        static event EVT evt;
        static int loop;
        static object LOCK;

        static void Main(string[] args)
        {
            DELG dlG_time = new DELG(time);
            DELG dlg_multiCast;
            IAsyncResult asyncR;
            LOCK = new object();
            loop = 0;
            evt += new EVT(state_display);
            var an_type = new { msg_pext = "pextension", msg_noext = "noextension" };
            Thread thd_timeInvok = new Thread(new ThreadStart(dlG_time.Invoke));

            Thread thd_paraExt = new Thread( new ThreadStart(() =>
                    {
                        Parallel.For(0, 10, i =>
                        {
                            Console.WriteLine(@"{0}", an_type.msg_pext);
                            Thread.Sleep(1000);
                        });
                        evt((object)an_type.msg_pext);
                    }));

            Thread thd_noParaExt = new Thread ( new ThreadStart(() =>
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            Console.WriteLine(@"{0}", an_type.msg_noext);
                            Thread.Sleep(1000);
                        };
                        evt((object)an_type.msg_noext);
                    }));

            asyncR = dlG_time.BeginInvoke((async) =>
            {
                DELG d = (DELG)((System.Runtime.Remoting.Messaging.AsyncResult)async).AsyncDelegate;
                d.EndInvoke(async);
                Console.Write("Fin des traveaux");
            }, dlG_time);

            dlg_multiCast = thd_paraExt.Start;
            dlg_multiCast += thd_noParaExt.Start;
            dlg_multiCast.Invoke();

            while (!asyncR.IsCompleted) 
            { 
                Console.WriteLine("Travaux en cours..."); 
                Thread.Sleep(5000); 
            }
            Console.Read();
        }

        static void time()
        {
            long t = 0;
            lock (LOCK)
            {
                while (loop < 2)
                {
                    Console.WriteLine(t.ToString());
                    t += 1;
                    Thread.Sleep(1000);
                }
            }
        }

        static void state_display(object o)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("{0} STOP", (string)o);
            Console.ForegroundColor = ConsoleColor.Gray;
            Interlocked.Increment(ref loop);
        }
    }
}

