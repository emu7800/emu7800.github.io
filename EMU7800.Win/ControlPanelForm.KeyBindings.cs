using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EMU7800.Core;
using EMU7800.Win.DirectX;

namespace EMU7800.Win
{
    partial class ControlPanelForm
    {
        #region Fields

        readonly IDictionary<string, string>
            _hostInputToKey = new Dictionary<string, string>(),
            _keyToHostInput = new Dictionary<string, string>();

        List<string> _unboundKeys = new List<string>();
        readonly List<string> _boundKeys = new List<string>();

        #endregion

        #region Event Handlers

        private void ComboboxKbHostInputSelectedIndexChanged(object sender, EventArgs e)
        {
            var boundKey = _hostInputToKey[(string)comboboxKbHostInput.SelectedItem];
            if ((string)comboboxKbKey.SelectedItem != boundKey)
            {
                comboboxKbKey.SelectedItem = boundKey;
            }
        }

        private void ComboboxKbKeySelectedIndexChanged(object sender, EventArgs e)
        {
            if (!comboboxKbHostInput.Enabled)
                return;
            var boundInput = _keyToHostInput[(string)comboboxKbKey.SelectedItem];
            if ((string)comboboxKbHostInput.SelectedItem != boundInput)
            {
                comboboxKbHostInput.SelectedItem = boundInput;
            }
        }

        private void ButtonKbBindActionClick(object sender, EventArgs e)
        {
            if (comboboxKbHostInput.Enabled)
            {
                StartUpdate();
            }
            else
            {
                CommitUpdate();
                EndUpdate();
            }
        }

        private void ButtonKbCancelClick(object sender, EventArgs e)
        {
            EndUpdate();
        }

        #endregion

        #region UI Helpers

        void InitializeKeyBindingsComboBoxes()
        {
            InitializeIndexesFromKeyBindings(HostBase.CreateDefaultKeyBindings());
            LoadKeyBindingsFromGlobalSetting();

            comboboxKbKey.DataSource = _boundKeys;

            var hostInputList = new List<string>(_hostInputToKey.Keys);
            hostInputList.Sort();
            comboboxKbHostInput.DataSource = hostInputList;
            comboboxKbHostInput.SelectedIndex = 0;

            EndUpdate();
        }

        void StartUpdate()
        {
            comboboxKbHostInput.Enabled = false;
            buttonKbCancel.Enabled = true;
            buttonKbBindAction.Text = "Bind";
            labelKbKey.Text = "Unbound DirectX Input Key";

            comboboxKbKey.DataSource = _unboundKeys;
        }

        void CommitUpdate()
        {
            var hostInput = (string)comboboxKbHostInput.SelectedItem;
            var newKey = (string)comboboxKbKey.SelectedItem;

            UpdateKeyBinding(hostInput, newKey);
            SaveKeyBindingsToGlobalSetting();
        }

        void EndUpdate()
        {
            comboboxKbHostInput.Enabled = true;
            buttonKbCancel.Enabled = false;
            buttonKbBindAction.Text = "Unbind";
            labelKbKey.Text = "Bound DirectX Input Key";

            var selectedInput = (string)comboboxKbHostInput.SelectedItem;
            comboboxKbKey.DataSource = _boundKeys;
            comboboxKbHostInput.SelectedItem = selectedInput;
        }

        #endregion

        #region Helpers

        private void InitializeIndexesFromKeyBindings(IDictionary<Key, MachineInput> keyBindings)
        {
            _unboundKeys = new List<string>(Enum.GetNames(typeof(Key)));
            foreach (var hostInput in Enum.GetNames(typeof(MachineInput)))
            {
                var key = (from kvPair in keyBindings
                           where hostInput == kvPair.Value.ToString()
                           select kvPair.Key.ToString()).FirstOrDefault();
                if (key == null)
                    continue;

                _hostInputToKey.Add(hostInput, key);
                _keyToHostInput.Add(key, hostInput);
                _boundKeys.Add(key);
                _unboundKeys.Remove(key);
            }
            _boundKeys.Sort();
            _unboundKeys.Sort();
        }

        void LoadKeyBindingsFromGlobalSetting()
        {
            foreach (var keyHostInput in _globalSettings.KeyBindings.Split(';')
                .Select(binding => binding.Split(','))
                .Where(keyHostInput => (keyHostInput.Length.Equals(2) && keyHostInput[0] != null) && keyHostInput[1] != null))
            {
                var hostInput = keyHostInput[1];
                var newKey = keyHostInput[0];

                MachineInput dummyHostInput;
                if (!Enum.TryParse(hostInput, true, out dummyHostInput))
                    continue;
                Key dummyKey;
                if (!Enum.TryParse(newKey, true, out dummyKey))
                    continue;
                var priorKey = _hostInputToKey[hostInput];
                if (priorKey == newKey)
                    continue;

                _hostInputToKey[hostInput] = newKey;
                _keyToHostInput.Remove(newKey);
                foreach (var kv in _keyToHostInput.Where(x => x.Value == hostInput).ToList())
                    _keyToHostInput.Remove(kv.Key);
                _keyToHostInput.Add(newKey, hostInput);

                _boundKeys.Remove(priorKey);
                _unboundKeys.Add(priorKey);
                _unboundKeys.Remove(newKey);
                _boundKeys.Add(newKey);
            }

            _boundKeys.Sort();
            _unboundKeys.Sort();
        }

        void SaveKeyBindingsToGlobalSetting()
        {
            var s = new StringBuilder();
            foreach (var keyVal in _keyToHostInput)
            {
                if (s.Length > 0) s.Append(";");
                s.AppendFormat("{0},{1}", keyVal.Key, keyVal.Value);
            }
            _globalSettings.KeyBindings = s.ToString();
        }

        void UpdateKeyBinding(string hostInput, string newKey)
        {
            if (hostInput == null || newKey == null)
                return;

            MachineInput machineInput;
            if (!Enum.TryParse(hostInput, true, out machineInput))
                return;
            Key key;
            if (!Enum.TryParse(newKey, true, out key))
                return;
            var priorKey = _hostInputToKey[hostInput];
            if (priorKey == newKey)
                return;

            _hostInputToKey[hostInput] = newKey;
            _keyToHostInput.Remove(priorKey);
            _keyToHostInput.Remove(newKey);
            foreach (var kv in _keyToHostInput.Where(x => x.Value == hostInput).ToList())
                _keyToHostInput.Remove(kv.Key);
            _keyToHostInput.Add(newKey, hostInput);

            _boundKeys.Remove(priorKey);
            _boundKeys.Add(newKey);
            _boundKeys.Sort();
            _unboundKeys.Add(priorKey);
            _unboundKeys.Remove(newKey);
            _unboundKeys.Sort();
        }

        #endregion
    }
}