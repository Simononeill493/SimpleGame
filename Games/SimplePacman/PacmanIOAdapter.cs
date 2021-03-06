﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleGame.DataPayloads.DiscreteData;

namespace SimpleGame.Games.SimplePacman
{
    class PacmanIOAdapter : IDiscreteGameIOAdapter
    {
        public IDiscreteDataPayload GetOutput(IDiscreteGameState genericState)
        {
            var state = (IPacmanInstance)genericState;

            return new DiscreteDataPayload(state.GetStatus());

        }

        public void SendInput(IDiscreteGameState genericState, IDiscreteDataPayload input)
        {
            var state = (IPacmanInstance)genericState;

            state.SendInput((Direction)input.Data[0]);
        }
    }
}
