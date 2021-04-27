using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Networking.TCP
{
    /// <summary>
    /// Serializes an object, making it ready
    /// for data transfer over the network
    /// </summary>
    /// <typeparam name="T">Type of the object</typeparam>
    public class Packet<T>
    {
        public string Data;

        public Packet(T obj)
        {
            if (!Serializable(obj))
            {
                throw new System.Exception
                (
                    "This object is not serializable. " +
                    "Add the [System.Serializable] attribute " +
                    "at the top of your object"
                );
            }

            var bytes = ConvertToBytes(obj);
            Data = Encoding.ASCII.GetString(bytes);
        }

        byte[] ConvertToBytes(T obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);

            return ms.ToArray();
        }

        bool Serializable(T obj)
        {
            var attributes = obj.GetType().CustomAttributes;

            foreach (var attribute in attributes)
            {
                if (attribute.AttributeType == typeof(System.SerializableAttribute))
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Static class for retrieving objects from a data stream
    /// </summary>
    public class Packet
    {
        /// <summary>
        /// Parses an object of a specified type from
        /// the received data of another client
        /// </summary>
        /// <typeparam name="T">The type of the original object</typeparam>
        /// <param name="data">The raw data string from the client</param>
        /// <returns></returns>
        public static T GetObject<T>(string data)
        {
            var bytes = Encoding.ASCII.GetBytes(data);

            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(bytes);

            var obj = bf.Deserialize(ms);
            return (T)obj;
        }

        /// <summary>
        /// Attemps to parse an object of a specified type from
        /// the received data of another client
        /// </summary>
        /// <typeparam name="T">The assumed orginal type of the object</typeparam>
        /// <param name="data">The raw data string from the client</param>
        /// <param name="onSuccess">The method invoked if parsing was successful</param>
        /// <param name="onFailure">The method invoked if parsing failed</param>
        public static void TryGetObject<T>(string data, Action<T> onSuccess, Action<string> onFailure = null)
        {
            try
            {
                var bytes = Encoding.ASCII.GetBytes(data);
                Console.WriteLine(data);

                BinaryFormatter bf = new BinaryFormatter();
                MemoryStream ms = new MemoryStream(bytes);

                var obj = bf.Deserialize(ms);

                var convertedObj = (T)obj;
                onSuccess?.Invoke(convertedObj);
            }
            catch (Exception e)
            {
                onFailure?.Invoke(e.Message);
            }
        }
    }
}
