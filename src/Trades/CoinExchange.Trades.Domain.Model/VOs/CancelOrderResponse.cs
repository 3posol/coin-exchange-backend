﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinExchange.Trades.Domain.Model.VOs
{
    /// <summary>
    /// VO to response in result of order request
    /// </summary>
    public class CancelOrderResponse
    {
       
        public CancelOrderResponse(bool pending, int count)
        {
            Pending = pending;
            Count = count;
        }

        //No. of order canceled
        public int Count { get; private set; }
        
        //if set, order(s) is/are pending cancellation
        public bool Pending { get; private set; }

    }
}
