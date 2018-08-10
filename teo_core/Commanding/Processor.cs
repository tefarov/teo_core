using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TEO.General.Messaging;

namespace TEO.Commanding
{
    public class Processor
    {
        /// <summary>
        /// This delegate is need to call commands in async threads
        /// </summary>
        /// <param name="command">A command to be launched</param>
        /// <param name="args">Args to be passed to a command</param>
        delegate ICommand command_call(ICommand command);

        /// <summary> Ui-thread owner </summary>
        //Form UIT;

        Environment.Environment ENV;
        Context CTX;

        /// <summary>
        /// This is used to display commands' text output
        /// </summary>
        public Provider Display;

        public bool IsRunning { get; private set; }

        public Processor(Environment.Environment environment)
        {
            ENV = environment.NotNull();
            CTX = new Context(ENV);
        }

        public void Execute(IBatch batch)
        {
            if (this.IsRunning) throw new InvalidOperationException("Processor is occupied at the moment");
            this.IsRunning = true;

            var sbch = new Stack<IEnumerator<IBatchable>>();
            var enm = batch.GetEnumerator();

            next_batch:
            while (enm.MoveNext()) {
                var bab = enm.Current;
                var position = bab.ToString();

                // exits this batch
                if (bab is BatchBreaker)
                    break;
                // the next elementt is a batch!
                // push the current into stack and process the next batch
                else if (bab is IBatch) {
                    try {
                        sbch.Push(enm);
                        enm = ((IBatch)bab).GetEnumerator();
                        goto next_batch;
                    }
                    catch (OperationCanceledException) { }
                    catch (ExceptionRuntime ex) {

                        var typ = ex.IsCritical ? TMessage.ExceptionCritical : TMessage.Warning;

                        if (this.Fail != null)
                            this.Fail(this, new EventArgs(ex, position));
                        else if (this.Display != null)
                            this.Display.Write(ex, typ, position: position);

                        if (ex.IsCritical) return;
                    }
                    catch (Exception ex) {
                        if (this.Fail != null)
                            this.Fail(this, new EventArgs(ex, position));
                        else if (this.Display != null)
                            this.Display.Write(ex, position: position);

                        return;
                    }
                }
                // next element is a comand !
                // comands should be processed
                else if (bab is ICommand) {
                    var exe = this.Execute((ICommand)bab);
                    if (exe.IsBreak) goto next_batch;
                    if (!exe.IsSuccessfull) goto fail;
                }
            }

            if (sbch.Any()) {
                enm = sbch.Pop();
                goto next_batch;
            }

            fail:
            this.IsRunning = false;
        }
        public ExecuteResult Execute(ICommand item)
        {
            var exe = new ExecuteResult(false);
            while (item != null) {
                var typ = item.Type;

                if (typ == TCommand.Sequential) {
                    exe = this.execute(item);
                    if (!exe.IsSuccessfull) return exe;
                    if (exe.IsBreak) return exe;
                }
                else if (typ == TCommand.Async) {
                    var itm = item; // this is needed because we may reassign the command variable before the thread starts
                    (new Task(() => { this.execute(itm); })).Start();
                }

                if (item is ICommandTunnelable)
                    item = ((ICommandTunnelable)item).Next;
                else
                    item = null;
            }
            return exe;
        }
        ExecuteResult execute(ICommand item, string position = null)
        {
            var rst = new ExecuteResult(false);
            var dsp = this.Display;
            var img = this.Display != null && item is IMessageable; // ismessageable

            if (img) ((IMessageable)item).Message += dsp.Write;
            if (dsp != null && item is IGui) dsp.Write(((IGui)item).Text, TMessage.CommandHeader);

            try {
                //if (this.Display != null) this.Display.Write(item.Description, TMessage.CommandHeader);

                rst = item.Execute(CTX);

                if (this.Success != null)
                    this.Success(this, new EventArgs(rst.Description, position));
                else if (this.Display != null)
                    this.Display.Write(rst.Description, TMessage.CommandResult);

            }
            catch (OperationCanceledException) { }
            catch (ExceptionRuntime ex) {

                var typ = ex.IsCritical ? TMessage.ExceptionCritical : TMessage.Warning;

                if (this.Fail != null)
                    this.Fail(this, new EventArgs(ex, position));
                else if (this.Display != null)
                    this.Display.Write(ex, typ, position: position);

                if (ex.IsCritical) rst = new ExecuteResult(false);
            }
            catch (Exception ex) {                

                if (this.Fail != null)
                    this.Fail(this, new EventArgs(ex, position));
                else if (this.Display != null)
                    this.Display.Write(ex, position: position);

                rst = new ExecuteResult(false);
            }

            if (img) ((IMessageable)item).Message -= dsp.Write;

            return rst;
        }

        public event EventHandler<EventArgs> Success, Fail;

        public class EventArgs : System.EventArgs
        {
            public EventArgs(string description, string position = null)
            {
                this.Description = description;
                this.Position = position;
            }
            public EventArgs(Exception ex, string position = null)
            {
                this.Exception = ex;
                this.Position = position;
            }

            public readonly Exception Exception;
            public readonly string Position;
            public readonly string Description;
        }
    }

}
