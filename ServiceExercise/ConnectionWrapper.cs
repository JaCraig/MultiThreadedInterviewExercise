using ConnectionPool;
using System;
using System.Threading.Tasks;

namespace ServiceExercise
{
    /// <summary>
    /// Connection wrapper
    /// </summary>
    /// <seealso cref="System.IDisposable"/>
    public class ConnectionWrapper : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionWrapper"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public ConnectionWrapper(Connection connection)
        {
            Connection = connection;
            CurrentTask = Task.FromResult(0);
        }

        /// <summary>
        /// Gets or sets the current task.
        /// </summary>
        /// <value>The current task.</value>
        public Task<int> CurrentTask { get; set; }

        /// <summary>
        /// Gets the connection.
        /// </summary>
        /// <value>The connection.</value>
        private Connection Connection { get; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (Connection is null)
                return;
            Connection.Dispose();

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Runs the command asynchronous.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public Task<int> runCommandAsync(int value)
        {
            CurrentTask = Connection.runCommandAsync(value);
            return CurrentTask;
        }
    }
}