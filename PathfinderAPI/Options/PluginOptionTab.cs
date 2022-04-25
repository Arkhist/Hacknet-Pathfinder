using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Pathfinder.GUI;

namespace Pathfinder.Options;

public class PluginOptionTab : IReadOnlyList<IPluginOption>
{
    private List<IPluginOption> options = new List<IPluginOption>();
    public string Id { get; private set; }
    public string TabName { get; private set; }
    public int HacknetGuiId { get; }
    public bool IsRegistered { get; internal set; }
    public PluginOptionDrawData ButtonData { get; set; } = new PluginOptionDrawData
    {
        X = 0,
        Y = 70,
        Width = 128,
        Height = 20
    };

    public PluginOptionTab(string tabName, string tabId = null)
    {
        TabName = tabName;
        Id = tabId ?? string.Concat(TabName.Where(c => !char.IsWhiteSpace(c) && c != '='));
        HacknetGuiId = PFButton.GetNextID();
    }

    public int Count => options.Count;

    public IPluginOption this[int index] => options[index];

    public PluginOptionTab AddOption(IPluginOption option)
    {
        if(options.Any(p => p.Id == option.Id))
            throw new InvalidOperationException("Option tabs may not have two options with the same id");
        options.Add(option);
        option.Tab = this;
        if(IsRegistered)
        {
            option.OnRegistered();
            _SetDrawPositions();
        }
        return this;
    }

    public IPluginOption GetOption(string id)
    {
        return options.Find(p => p.Id == id);
    }

    public OptionT GetOption<OptionT>(string id) where OptionT : IPluginOption
        => (OptionT)GetOption(id);

    public OptionT GetOptionAs<OptionT>(string id) where OptionT : class, IPluginOption
        => GetOption(id) as OptionT;

    public bool RemoveOption(IPluginOption option)
    {
        return options.Remove(option);
    }

    public bool RemoveOption(string id)
    {
        return RemoveOption(options.Find(p => p.Id == id));
    }

    public virtual void OnRegistered()
    {
        foreach(var opt in options)
            opt.OnRegistered();
        _SetDrawPositions();
    }

    public virtual void OnSave(ConfigFile config)
    {
        foreach(var opt in options)
            opt.OnSave(config);
    }

    public virtual void OnLoad(ConfigFile config)
    {
        foreach(var opt in options)
            opt.OnLoad(config);
    }

    public virtual void OnDraw(GameTime gameTime)
    {
        if (PathfinderOptionsMenu.CurrentTabId == null)
            PathfinderOptionsMenu.SetCurrentTab(this);
        var active = PathfinderOptionsMenu.CurrentTabId == Id;
        // Display tab button
        if (Button.doButton(HacknetGuiId,
            ButtonData.X.GetValueOrDefault(),
            ButtonData.Y.GetValueOrDefault(),
            ButtonData.Width.GetValueOrDefault(),
            ButtonData.Height.GetValueOrDefault(),
            TabName,
            active ? Color.Green : Color.Gray))
        {
            PathfinderOptionsMenu.SetCurrentTab(this);
            return;
        }

        if (PathfinderOptionsMenu.CurrentTabId != Id)
            return;

        // Display options
        foreach (var option in this)
            option.OnDraw(gameTime);
    }

    public IEnumerator<IPluginOption> GetEnumerator()
        => options.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private void _SetDrawPositions()
    {
        int defOptionXPos = ButtonData.Rectangle.Bottom, defOptionYPos = 110;
        foreach (var option in this)
        {
            if(option.DrawData.X == null)
                option.DrawData = option.DrawData.Set(defOptionXPos);
            if(option.DrawData.Y == null)
                option.DrawData = option.DrawData.Set(y: defOptionYPos);
            defOptionYPos += 10 + option.DrawData.Height.GetValueOrDefault();
        }
    }
}