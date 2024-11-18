using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ES.Shared.Exceptions;
public class ApiEndPointException : CustomException
{
    public ApiEndPointException()
    {
            
    }
    public ApiEndPointException(string message) : base(message) { }

    public ApiEndPointException(string message, Exception innerException) : base(message, innerException) { }
}
