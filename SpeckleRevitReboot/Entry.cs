﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SpeckleRevit.UI;
using SpeckleUiBase;

namespace SpeckleRevit
{
  public class SpecklePlugin : IExternalApplication
  {
    public Result OnShutdown( UIControlledApplication application )
    {
      return Result.Succeeded;
    }

    public Result OnStartup( UIControlledApplication application )
    {
      var SpecklePanel = application.CreateRibbonPanel( "Speckle" );
      var SpeckleButton = SpecklePanel.AddItem( new PushButtonData( "Speckle", "Speckle Revit", typeof( SpecklePlugin ).Assembly.Location, "SpeckleRevit.SpeckleRevitCommand" ) ) as PushButton;

      SpeckleButton.SetContextualHelp( new ContextualHelp( ContextualHelpType.Url, "https://speckle.works" ) );

      return Result.Succeeded;
    }
  }

  [Transaction( TransactionMode.Manual )]
  public class SpeckleRevitCommand : IExternalCommand
  {
    public static bool Launched = false;
    public static SpeckleUiWindow SpeckleWindow;

    public Result Execute( ExternalCommandData commandData, ref string message, ElementSet elements )
    {
      if ( !Launched )
      {

        var bindings = new SpeckleUiBindingsRevit( commandData.Application );
        var eventHandler = ExternalEvent.Create( new SpeckleRevitExternalEventHandler( bindings ) );
        bindings.Executor = eventHandler;


        SpeckleWindow = new SpeckleUiWindow( bindings );
        // TODO: find a way to set the parent/owner of the speckle window so it minimises/maximises etc. together with the revit window.
        SpeckleWindow.Show();
        Launched = true;
      }

      SpeckleWindow.Show();
      SpeckleWindow.Focus();

      return Result.Succeeded;
    }
  }

  public class SpeckleRevitExternalEventHandler : IExternalEventHandler
  {

    public SpeckleUiBindingsRevit myBindings { get; set; }
    public bool Running = false;

    public SpeckleRevitExternalEventHandler( SpeckleUiBindingsRevit _uiBindings )
    {
      myBindings = _uiBindings;
    }

    public void Execute( UIApplication app )
    {
      Debug.WriteLine( "Current queue len is: " + myBindings.Queue.Count );
      if ( Running ) return; // queue will run itself through

      Running = true;
      try
      {
        myBindings.Queue[ 0 ]();
      }
      catch ( Exception e )
      {
        Debug.WriteLine( e.Message );
      }

      myBindings.Queue.RemoveAt( 0 );
      Running = false;

      if ( myBindings.Queue.Count != 0 )
        myBindings.Executor.Raise();
    }

    public string GetName( )
    {
      return "SpeckleSmackleSpockle";
    }
  }

}
