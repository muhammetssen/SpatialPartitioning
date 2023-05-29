// Throughout the project, we have been using different approaches to handle different approaches and methodogies.
// This enum is used to identify the different types of solutions we have used in the project.
// AOI: Area of Interest
public enum SolutionTypes {
    Naive, // servers send everything to all other servers. Client is connected to one server and receives everything from that server.
    NaiveWithAOI,  // servers send everything to all other servers. Client is connected to one server and receives only the objects in its AOI from that server.
    ServerBuffering, // servers only share objects in the buffer zone with other servers. Client is connected to one server and receives everything from that server.
    ServerBufferingWithAOI, // servers only share objects in the buffer zone with other servers. Client is connected to one server and receives only the objects in its AOI from that server.
    Neighbourhood, // servers share objects with their neighbours. Client is connected to one server and receives everything from that server.
    // NeighbourhoodWithAOI, // servers share objects with their neighbours. Client is connected to one server and receives only the objects in its AOI from that server.

}