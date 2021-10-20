﻿using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace ClickLib
{
    /// <summary>
    /// Searchable click base class.
    /// </summary>
    public abstract unsafe class ClickBase
    {
        /// <summary>
        /// AtkEventListener receive event delegate.
        /// </summary>
        /// <param name="eventListener">Type receiving the event.</param>
        /// <param name="evt">Event type.</param>
        /// <param name="which">Internal routing number.</param>
        /// <param name="eventData">Event data.</param>
        /// <param name="inputData">Keyboard and mouse data.</param>
        /// <returns>The event listener address.</returns>
        internal unsafe delegate IntPtr ReceiveEventDelegate(AtkEventListener* eventListener, EventType evt, uint which, void* eventData, void* inputData);

        /// <summary>
        /// Invoke the receive event delegate.
        /// </summary>
        /// <param name="eventListener">Type receiving the event.</param>
        /// <param name="type">Event type.</param>
        /// <param name="which">Internal routing number.</param>
        /// <param name="eventData">Event data.</param>
        /// <param name="inputData">Keyboard and mouse data.</param>
        protected static void InvokeReceiveEvent(AtkEventListener* eventListener, EventType type, uint which, EventData eventData, InputData inputData)
        {
            var receiveEvent = GetReceiveEvent(eventListener);
            receiveEvent(eventListener, type, which, eventData.Data, inputData.Data);
        }

        /// <inheritdoc cref="InvokeReceiveEvent(AtkEventListener*, EventType, uint, EventData, InputData)"/>
        protected static void InvokeReceiveEvent(AtkComponentBase* eventListener, EventType type, uint which, EventData eventData, InputData inputData)
            => InvokeReceiveEvent(&eventListener->AtkEventListener, type, which, eventData, inputData);

        private static ReceiveEventDelegate GetReceiveEvent(AtkEventListener* listener)
        {
            var receiveEventAddress = new IntPtr(listener->vfunc[2]);
            return Marshal.GetDelegateForFunctionPointer<ReceiveEventDelegate>(receiveEventAddress)!;
        }

        /// <summary>
        /// Event data.
        /// </summary>
        public sealed class EventData : IDisposable
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="EventData"/> class.
            /// </summary>
            private EventData()
            {
                this.Data = (void**)Marshal.AllocHGlobal(0x18).ToPointer();
                this.Data[0] = null;
                this.Data[1] = null;
                this.Data[2] = null;
            }

            /// <summary>
            /// Gets the data pointer.
            /// </summary>
            public void** Data { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="EventData"/> class.
            /// </summary>
            /// <param name="target">Target.</param>
            /// <param name="listener">Event listener.</param>
            /// <returns>Event data.</returns>
            public static EventData ForNormalTarget(void* target, void* listener)
            {
                var data = new EventData();
                data.Data[1] = target;
                data.Data[2] = listener;
                return data;
            }

            /// <inheritdoc/>
            public void Dispose()
            {
                Marshal.FreeHGlobal((IntPtr)this.Data);
            }
        }

        /// <summary>
        /// Input data.
        /// </summary>
        public sealed class InputData : IDisposable
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="InputData"/> class.
            /// </summary>
            private InputData()
            {
                this.Data = (void**)Marshal.AllocHGlobal(0x40).ToPointer();
                this.Data[0] = null;
                this.Data[1] = null;
                this.Data[2] = null;
                this.Data[3] = null;
                this.Data[4] = null;
                this.Data[5] = null;
                this.Data[6] = null;
                this.Data[7] = null;
            }

            /// <summary>
            /// Gets the data pointer.
            /// </summary>
            public void** Data { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="InputData"/> class.
            /// </summary>
            /// <returns>Input data.</returns>
            public static InputData Empty()
            {
                return new InputData();
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="InputData"/> class.
            /// </summary>
            /// <param name="popupMenu">List popup menu.</param>
            /// <param name="index">Selected index.</param>
            /// <returns>Input data.</returns>
            public static InputData ForPopupMenu(PopupMenu* popupMenu, ushort index)
            {
                var data = new InputData();
                data.Data[0] = popupMenu->List->ItemRendererList[index].AtkComponentListItemRenderer;
                data.Data[2] = (void*)(index | ((ulong)index << 48));
                return data;
            }

            /// <inheritdoc/>
            public void Dispose()
            {
                Marshal.FreeHGlobal((IntPtr)this.Data);
            }
        }
    }
}