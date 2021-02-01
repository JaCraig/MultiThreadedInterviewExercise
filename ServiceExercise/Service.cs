using ConnectionPool;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceExercise
{
    /// <summary>
    /// Service class
    /// </summary>
    /// <seealso cref="IService"/>
    /// <seealso cref="IDisposable"/>
    public class Service : IService, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Service"/> class.
        /// </summary>
        /// <param name="CONNETION_COUNT">The connection count.</param>
        public Service(int CONNETION_COUNT)
        {
            Connections = new ConnectionPool(CONNETION_COUNT);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="Service"/> class.
        /// </summary>
        ~Service()
        {
            Dispose();
        }

        /// <summary>
        /// Gets the connections.
        /// </summary>
        /// <value>The connections.</value>
        private ConnectionPool Connections { get; set; }

        /// <summary>
        /// Gets the requests.
        /// </summary>
        /// <value>The requests.</value>
        private BlockingCollection<Request> Requests { get; set; } = new BlockingCollection<Request>();

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Service"/> is running.
        /// </summary>
        /// <value><c>true</c> if running; otherwise, <c>false</c>.</value>
        private bool Running { get; set; }

        /// <summary>
        /// Gets the tasks.
        /// </summary>
        /// <value>The tasks.</value>
        private List<Task<int>> Tasks { get; } = new List<Task<int>>();

        /// <summary>
        /// The lock object
        /// </summary>
        private readonly object LockObject = new object();

        /// <summary>
        /// The finished loading
        /// </summary>
        private bool FinishedLoading;

        /// <summary>
        /// The summary available
        /// </summary>
        private bool SummaryAvailable;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (Connections is null)
                return;
            Connections.Dispose();
            Connections = null;

            Requests.Dispose();
            Requests = null;
        }

        /// <summary>
        /// returns the sum in the current time of the service
        /// </summary>
        /// <returns>The sum.</returns>
        public int getSummary()
        {
            while (!SummaryAvailable) ;
            Task.WaitAll(Tasks.ToArray());
            return Tasks.Sum(x => x.Result);
        }

        /// <summary>
        /// Notifies the finished loading.
        /// </summary>
        public void notifyFinishedLoading()
        {
            FinishedLoading = true;
            Console.WriteLine("Finished loading");
        }

        /// <summary>
        /// Produces this instance.
        /// </summary>
        public void Produce()
        {
            while (!FinishedLoading || Requests.Count > 0)
            {
                if (!Requests.TryTake(out var Request))
                    continue;
                Console.WriteLine(Request.Command + " has entered the Produce function");
                Tasks.Add(Connections.ProcessRequest(Request));
            }
            SummaryAvailable = true;
        }

        /// <summary>
        /// Adds requests to the queue, and runs the produce function one at a time
        /// </summary>
        /// <param name="request"></param>
        public void sendRequest(Request request)
        {
            Requests.Add(request);
            Console.WriteLine(request.Command + " has entered the queue");
            if (Running)
                return;
            lock (LockObject)
            {
                if (Running)
                    return;
                Running = true;
            }
            Task.Run(() => Produce());
        }
    }
}