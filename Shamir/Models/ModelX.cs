using System;
using System.Collections.Generic;
using System.Text;

namespace Shamir.Models
{
    public class ModelX
    {
        public long A = 1; 
        public long Power = 1;
        //A * x^Power
        public ModelX(long a, long power)
        {
            A = a;
            Power = power;
        }
        
    }
   
    
}
