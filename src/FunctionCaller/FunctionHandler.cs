﻿using System;
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

            //Read inputStream into HttpRequestMessage
            var requestMsg = LambdaStreamConverter.GetRequestMessage(inputStream);

            //Locate Run method
            var type = dotNetFunctionAsm.GetType("Function", true);
            var instance = Activator.CreateInstance(type);
            var method = type.GetMethod("Run");

            //Execute function
            Object result;
            try
            {
                result = method.Invoke(instance, new object[] { requestMsg });
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

            if (result == null || result.GetType() == typeof(Task))
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

            //Return result stream
            var respString = LambdaStreamConverter.GetResponseString((result as HttpResponseMessage) ?? new HttpResponseMessage());
            return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(respString));
        }
    }
}
