using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace FunctionHandler
{
    public class FunctionCaller
    {
        public Stream Invoke(Stream inputStream)
        {
            const string FUNCTION_ASSEMBLY_NAME = "DotNetFunction";

            var functionHandlerAsm = typeof(FunctionCaller).GetTypeInfo().Assembly; //this assembly

            //Function assembly is in a subfolder named {FUNCTION_ASSEMBLY_NAME}
            string functionPath = Path.Combine(Path.GetDirectoryName(functionHandlerAsm.Location), FUNCTION_ASSEMBLY_NAME, FUNCTION_ASSEMBLY_NAME + ".dll");

            //Load callee assembly
            var asmLoadCtx = System.Runtime.Loader.AssemblyLoadContext.GetLoadContext(functionHandlerAsm);
            Assembly dotNetFunctionAsm = asmLoadCtx.LoadFromAssemblyPath(functionPath);

            //TODO: Read inputStream into HttpRequestMessage

            var requestMsg = new HttpRequestMessage();

            //Locate Run method
            var type = dotNetFunctionAsm.GetType("Function", true);
            //var instance = Activator.CreateInstance(type);
            var method = type.GetMethod("Run");

            //Execute function
            Object result;
            try
            {
                result = method.Invoke(null /* instance is ignored for a static Run method */, new object[] { requestMsg });
            }
            catch(TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                {
                    throw ex.InnerException;
                }
                else
                {
                    throw;
                }
            }

            if (result == null)
            {
                //If result of call is null, present empty HttpResponseMessage
                result = new HttpResponseMessage();
            }

            //If result is a task, then wait for result.
            if (result.GetType() == typeof(Task<HttpResponseMessage>))
            {
                ((Task<HttpResponseMessage>)result).Wait();
                result = ((Task<HttpResponseMessage>)result).Result;
            }

            //TODO: Parse result into result stream

            var resp = "{  \"isBase64Encoded\": false, \"statusCode\": 200, \"body\": \"" + (result as HttpResponseMessage).Content.ReadAsStringAsync().Result + "\" }";

            return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(resp));


        }
    }
}
