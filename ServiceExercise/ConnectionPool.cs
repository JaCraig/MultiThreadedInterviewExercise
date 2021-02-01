using ConnectionPool;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceExercise
{
    /// <summary>
    /// Connection pool
    /// </summary>
    /// <seealso cref="IDisposable"/>
    public class ConnectionPool : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionPool"/> class.
        /// </summary>
        /// <param name="maxConnections">The maximum connections.</param>
        public ConnectionPool(int maxConnections)
        {
            Connections = new ConnectionWrapper[maxConnections];
            for (int x = 0; x < maxConnections; ++x)
            {
                Connections[x] = new ConnectionWrapper(new Connection());
            }
        }

        /// <summary>
        /// Gets the connections.
        /// </summary>
        /// <value>The connections.</value>
        private ConnectionWrapper[] Connections { get; set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (Connections is null)
                return;
            foreach (var Connection in Connections)
            {
                Connection.Dispose();
            }
            Connections = null;

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Processes the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>Task.</returns>
        public Task<int> ProcessRequest(Request request)
        {
            var ConnectionToUse = Task.WaitAny(Connections.Select(x => x.CurrentTask).ToArray());
            return Connections[ConnectionToUse].runCommandAsync(request.Command);
        }
    }
}