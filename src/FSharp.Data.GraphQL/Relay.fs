﻿/// The MIT License (MIT)
/// Copyright (c) 2016 Bazinga Technologies Inc
module FSharp.Data.GraphQL.Relay

open FSharp.Data.GraphQL.Types

type Edge<'Node> = 
    { Cursor : string
      Node : 'Node }

type PageInfo = 
    { HasNextPage : bool
      HasPreviousPage : bool
      StartCursor : string
      EndCursor : string }

type Connection<'Node> = 
    { TotalCount : int
      PageInfo : PageInfo
      Edges : Edge<'Node> list }
    member x.Items = x.Edges |> List.map (fun e -> e.Node)

let Edge nodeType = Schema.ObjectType(
    name = nodeType.ToString() + "Edge",
    description = "An edge in a connection from an object to another object of type " + nodeType.ToString(),
    fields = [
        Schema.Field("cursor", NonNull String, fun edge -> edge.Cursor, "A cursor for use in pagination")
        Schema.Field("node", nodeType, fun edge -> edge.Node, "The item at the end of the edge")
    ]) 

let PageInfo = Schema.ObjectType(
    name = "PageInfo",
    description = "Information about pagination in a connection.",
    fields = [
        Schema.Field("hasNextPage", NonNull Bool, fun pageInfo -> pageInfo.HasNextPage, "When paginating forwards, are there more items?")
        Schema.Field("hasPreviousPage", NonNull Bool, fun pageInfo -> pageInfo.HasPreviousPage, "When paginating backwards, are there more items?")
        Schema.Field("startCursor", String, fun pageInfo -> pageInfo.StartCursor, "When paginating backwards, the cursor to continue.")
        Schema.Field("endCursor ", String, fun pageInfo -> pageInfo.EndCursor, "When paginating forwards, the cursor to continue.")
    ])

let ConnectionType nodeType = Schema.ObjectType(
    name = nodeType.ToString(),
    description = "A connection from an object to a list of objects of type " + nodeType.ToString(),
    fields = [
        Schema.Field("totalCount", Int, fun conn -> conn.TotalCount , """A count of the total number of objects in this connection, ignoring pagination. 
                    This allows a client to fetch the first five objects by passing \"5\" as the argument 
                    to `first`, then fetch the total count so it could display \"5 of 83\", for example. 
                    In cases where we employ infinite scrolling or don't have an exact count of entries, 
                    this field will return `null`.""")
        Schema.Field("pageInfo", NonNull PageInfo, fun conn -> conn.PageInfo, "Information to aid in pagination.")
        Schema.Field("edges", ListOf(Edge nodeType), fun conn -> conn.Edges, "Information to aid in pagination.")
        Schema.Field("items", ListOf(nodeType), fun (conn: Connection<_>) -> conn.Items, """A list of all of the objects returned in the connection. This is a convenience field provided 
                    for quickly exploring the API; rather than querying for \"{ edges { node } }\" when no edge data 
                    is needed, this field can be used instead. Note that when clients like Relay need to fetch 
                    the \"cursor\" field on the edge to enable efficient pagination, this shortcut cannot be used, 
                    and the full \"{ edges { node } } \" version should be used instead.""")
    ])