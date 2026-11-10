# -*- coding: utf-8 -*-
import xml.etree.ElementTree as ET

def generate_diagrams():
    # File 1: VI.1 - SOA Architecture Diagrams
    xml_vi1 = '''<mxfile host="app.diagrams.net" version="24.0.0" type="device">
  <diagram id="vi1-component" name="1. Component Diagram (SOA)">
    <mxGraphModel dx="1200" dy="800" grid="1" gridSize="10" guides="1" tooltips="1" connect="1" arrows="1" fold="1" page="1" pageScale="1" pageWidth="1200" pageHeight="850" math="0" shadow="0">
      <root>
        <mxCell id="0" />
        <mxCell id="1" parent="0" />
        
        <!-- Title -->
        <mxCell id="title" value="&lt;b&gt;&lt;font style=&quot;font-size: 16px;&quot; color=&quot;#1A365D&quot;&gt;VI.1 Component Diagram - Pet Care Service-Oriented Architecture (SOA)&lt;/font&gt;&lt;/b&gt;" style="text;html=1;strokeColor=none;fillColor=none;align=center;verticalAlign=middle;whiteSpace=wrap;rounded=0;" vertex="1" parent="1">
          <mxGeometry x="250" y="20" width="700" height="30" as="geometry" />
        </mxCell>

        <!-- Client Layer -->
        <mxCell id="c_client_box" value="Frontend &amp; Client Tier" style="swimlane;whiteSpace=wrap;html=1;fillColor=#f5f5f5;fontColor=#333333;strokeColor=#666666;fontStyle=1;" vertex="1" parent="1">
          <mxGeometry x="50" y="70" width="1100" height="90" as="geometry" />
        </mxCell>
        <mxCell id="c_web_client" value="&amp;laquo;component&amp;raquo;&#xa;&lt;b&gt;Customer Web App&lt;/b&gt;&#xa;(React / Next.js)" style="shape=module;jettyWidth=8;jettyHeight=4;whiteSpace=wrap;html=1;fillColor=#dae8fc;strokeColor=#6c8ebf;fontStyle=0;" vertex="1" parent="c_client_box">
          <mxGeometry x="150" y="30" width="180" height="50" as="geometry" />
        </mxCell>
        <mxCell id="c_admin_client" value="&amp;laquo;component&amp;raquo;&#xa;&lt;b&gt;Admin &amp; Staff Portal&lt;/b&gt;&#xa;(React / Blazor)" style="shape=module;jettyWidth=8;jettyHeight=4;whiteSpace=wrap;html=1;fillColor=#dae8fc;strokeColor=#6c8ebf;fontStyle=0;" vertex="1" parent="c_client_box">
          <mxGeometry x="450" y="30" width="180" height="50" as="geometry" />
        </mxCell>
        <mxCell id="c_pos_client" value="&amp;laquo;component&amp;raquo;&#xa;&lt;b&gt;Mobile / POS Client&lt;/b&gt;&#xa;(Future Expansion)" style="shape=module;jettyWidth=8;jettyHeight=4;whiteSpace=wrap;html=1;fillColor=#dae8fc;strokeColor=#6c8ebf;fontStyle=0;strokeDasharray=3 3;" vertex="1" parent="c_client_box">
          <mxGeometry x="750" y="30" width="180" height="50" as="geometry" />
        </mxCell>

        <!-- API Gateway -->
        <mxCell id="c_gateway" value="&amp;laquo;component&amp;raquo;&#xa;&lt;b&gt;API Gateway (Ocelot / YARP)&lt;/b&gt;&#xa;[Reverse Proxy | Auth Verification | Rate Limiting | SSL]" style="shape=module;jettyWidth=10;jettyHeight=6;whiteSpace=wrap;html=1;fillColor=#ffe6cc;strokeColor=#d79b00;fontStyle=0;fontSize=12;" vertex="1" parent="1">
          <mxGeometry x="350" y="190" width="500" height="60" as="geometry" />
        </mxCell>

        <!-- Client to Gateway Connectors -->
        <mxCell id="e_c1_gw" style="edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;entryX=0.25;entryY=0;entryDx=0;entryDy=0;" edge="1" parent="1" source="c_web_client" target="c_gateway">
          <mxGeometry relative="1" as="geometry" />
        </mxCell>
        <mxCell id="e_c2_gw" style="edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;entryX=0.5;entryY=0;entryDx=0;entryDy=0;" edge="1" parent="1" source="c_admin_client" target="c_gateway">
          <mxGeometry relative="1" as="geometry" />
        </mxCell>
        <mxCell id="e_c3_gw" style="edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;entryX=0.75;entryY=0;entryDx=0;entryDy=0;strokeDasharray=3 3;" edge="1" parent="1" source="c_pos_client" target="c_gateway">
          <mxGeometry relative="1" as="geometry" />
        </mxCell>

        <!-- Microservices Scope -->
        <mxCell id="c_services_box" value="Autonomous Business Services Layer (SOA)" style="swimlane;whiteSpace=wrap;html=1;fillColor=#f5f5f5;fontColor=#333333;strokeColor=#666666;fontStyle=1;" vertex="1" parent="1">
          <mxGeometry x="50" y="280" width="1100" height="230" as="geometry" />
        </mxCell>
        
        <mxCell id="c_svc_auth" value="&amp;laquo;service&amp;raquo;&#xa;&lt;b&gt;Identity &amp; User Service&lt;/b&gt;&#xa;&lt;font style=&quot;font-size: 10px;&quot;&gt;• AuthController, JWT&#xa;• Customers &amp; Staffs&#xa;• Roles &amp; Permissions&lt;/font&gt;" style="shape=module;jettyWidth=8;jettyHeight=4;whiteSpace=wrap;html=1;fillColor=#d5e8d4;strokeColor=#82b366;align=center;verticalAlign=middle;" vertex="1" parent="c_services_box">
          <mxGeometry x="20" y="40" width="190" height="85" as="geometry" />
        </mxCell>

        <mxCell id="c_svc_pet" value="&amp;laquo;service&amp;raquo;&#xa;&lt;b&gt;Pet &amp; Care Record Service&lt;/b&gt;&#xa;&lt;font style=&quot;font-size: 10px;&quot;&gt;• Pets, Health Profiles&#xa;• CareRecords (Care Logs)&#xa;• Cloudinary Uploads&lt;/font&gt;" style="shape=module;jettyWidth=8;jettyHeight=4;whiteSpace=wrap;html=1;fillColor=#d5e8d4;strokeColor=#82b366;align=center;verticalAlign=middle;" vertex="1" parent="c_services_box">
          <mxGeometry x="240" y="40" width="190" height="85" as="geometry" />
        </mxCell>

        <mxCell id="c_svc_room" value="&amp;laquo;service&amp;raquo;&#xa;&lt;b&gt;Room &amp; Facility Service&lt;/b&gt;&#xa;&lt;font style=&quot;font-size: 10px;&quot;&gt;• Rooms &amp; RoomTypes&#xa;• Availability Checking&#xa;• Cage &amp; Slot Inventory&lt;/font&gt;" style="shape=module;jettyWidth=8;jettyHeight=4;whiteSpace=wrap;html=1;fillColor=#d5e8d4;strokeColor=#82b366;align=center;verticalAlign=middle;" vertex="1" parent="c_services_box">
          <mxGeometry x="460" y="40" width="190" height="85" as="geometry" />
        </mxCell>

        <mxCell id="c_svc_booking" value="&amp;laquo;service&amp;raquo;&#xa;&lt;b&gt;Booking &amp; Order Service&lt;/b&gt;&#xa;&lt;font style=&quot;font-size: 10px;&quot;&gt;• Bookings &amp; Items&#xa;• Services &amp; Vouchers&#xa;• Reviews &amp; Feedback&lt;/font&gt;" style="shape=module;jettyWidth=8;jettyHeight=4;whiteSpace=wrap;html=1;fillColor=#d5e8d4;strokeColor=#82b366;align=center;verticalAlign=middle;" vertex="1" parent="c_services_box">
          <mxGeometry x="680" y="40" width="190" height="85" as="geometry" />
        </mxCell>

        <mxCell id="c_svc_payment" value="&amp;laquo;service&amp;raquo;&#xa;&lt;b&gt;Payment &amp; Notify Service&lt;/b&gt;&#xa;&lt;font style=&quot;font-size: 10px;&quot;&gt;• Payments, Transactions&#xa;• VnPayService Gateway&#xa;• EmailService (SMTP)&lt;/font&gt;" style="shape=module;jettyWidth=8;jettyHeight=4;whiteSpace=wrap;html=1;fillColor=#d5e8d4;strokeColor=#82b366;align=center;verticalAlign=middle;" vertex="1" parent="c_services_box">
          <mxGeometry x="890" y="40" width="190" height="85" as="geometry" />
        </mxCell>

        <!-- Message Broker -->
        <mxCell id="c_eventbus" value="&amp;laquo;event bus&amp;raquo;&#xa;&lt;b&gt;Message Broker (RabbitMQ / Kafka)&lt;/b&gt;&#xa;[Topics: BookingCreatedEvent, PaymentCompletedEvent, DailyCareUpdatedEvent]" style="shape=module;jettyWidth=8;jettyHeight=4;whiteSpace=wrap;html=1;fillColor=#e1d5e7;strokeColor=#9673a6;align=center;verticalAlign=middle;" vertex="1" parent="c_services_box">
          <mxGeometry x="240" y="155" width="630" height="55" as="geometry" />
        </mxCell>

        <!-- Async connections to Event bus -->
        <mxCell id="e_bkg_bus" style="edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;entryX=0.75;entryY=0;entryDx=0;entryDy=0;strokeColor=#9673a6;strokeWidth=1.5;strokeDasharray=4 4;" edge="1" parent="c_services_box" source="c_svc_booking" target="c_eventbus">
          <mxGeometry relative="1" as="geometry" />
        </mxCell>
        <mxCell id="e_bus_pay" style="edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;entryX=0.3;entryY=1;entryDx=0;entryDy=0;strokeColor=#9673a6;strokeWidth=1.5;strokeDasharray=4 4;" edge="1" parent="c_services_box" source="c_eventbus" target="c_svc_payment">
          <mxGeometry relative="1" as="geometry" />
        </mxCell>
        <mxCell id="e_bus_pet" style="edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;entryX=0.5;entryY=1;entryDx=0;entryDy=0;strokeColor=#9673a6;strokeWidth=1.5;strokeDasharray=4 4;" edge="1" parent="c_services_box" source="c_eventbus" target="c_svc_pet">
          <mxGeometry relative="1" as="geometry" />
        </mxCell>

        <!-- Gateway to Services Routing -->
        <mxCell id="e_gw_s1" style="edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;entryX=0.5;entryY=0;entryDx=0;entryDy=0;" edge="1" parent="1" source="c_gateway" target="c_svc_auth">
          <mxGeometry relative="1" as="geometry" />
        </mxCell>
        <mxCell id="e_gw_s2" style="edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;entryX=0.5;entryY=0;entryDx=0;entryDy=0;" edge="1" parent="1" source="c_gateway" target="c_svc_pet">
          <mxGeometry relative="1" as="geometry" />
        </mxCell>
        <mxCell id="e_gw_s3" style="edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;entryX=0.5;entryY=0;entryDx=0;entryDy=0;" edge="1" parent="1" source="c_gateway" target="c_svc_room">
          <mxGeometry relative="1" as="geometry" />
        </mxCell>
        <mxCell id="e_gw_s4" style="edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;entryX=0.5;entryY=0;entryDx=0;entryDy=0;" edge="1" parent="1" source="c_gateway" target="c_svc_booking">
          <mxGeometry relative="1" as="geometry" />
        </mxCell>
        <mxCell id="e_gw_s5" style="edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;entryX=0.5;entryY=0;entryDx=0;entryDy=0;" edge="1" parent="1" source="c_gateway" target="c_svc_payment">
          <mxGeometry relative="1" as="geometry" />
        </mxCell>

        <!-- Database Layer -->
        <mxCell id="c_db_box" value="Database Tier (Decoupled Database-per-Service Pattern)" style="swimlane;whiteSpace=wrap;html=1;fillColor=#f5f5f5;fontColor=#333333;strokeColor=#666666;fontStyle=1;" vertex="1" parent="1">
          <mxGeometry x="50" y="540" width="1100" height="150" as="geometry" />
        </mxCell>
        <mxCell id="db_auth" value="&lt;b&gt;Identity DB&lt;/b&gt;&#xa;(SQL / Postgres)&#xa;&lt;font style=&quot;font-size: 10px;&quot;&gt;Users, Roles, Staffs&lt;/font&gt;" style="shape=cylinder3;whiteSpace=wrap;html=1;boundedLbl=1;backgroundOutline=1;size=15;fillColor=#fff2cc;strokeColor=#d6b656;" vertex="1" parent="c_db_box">
          <mxGeometry x="50" y="40" width="130" height="80" as="geometry" />
        </mxCell>
        <mxCell id="db_pet" value="&lt;b&gt;Pet &amp; Care DB&lt;/b&gt;&#xa;(Postgres / MongoDB)&#xa;&lt;font style=&quot;font-size: 10px;&quot;&gt;Pets, CareRecords&lt;/font&gt;" style="shape=cylinder3;whiteSpace=wrap;html=1;boundedLbl=1;backgroundOutline=1;size=15;fillColor=#fff2cc;strokeColor=#d6b656;" vertex="1" parent="c_db_box">
          <mxGeometry x="270" y="40" width="130" height="80" as="geometry" />
        </mxCell>
        <mxCell id="db_room" value="&lt;b&gt;Room DB&lt;/b&gt;&#xa;(Postgres)&#xa;&lt;font style=&quot;font-size: 10px;&quot;&gt;Rooms, RoomTypes&lt;/font&gt;" style="shape=cylinder3;whiteSpace=wrap;html=1;boundedLbl=1;backgroundOutline=1;size=15;fillColor=#fff2cc;strokeColor=#d6b656;" vertex="1" parent="c_db_box">
          <mxGeometry x="490" y="40" width="130" height="80" as="geometry" />
        </mxCell>
        <mxCell id="db_bkg" value="&lt;b&gt;Booking DB&lt;/b&gt;&#xa;(Postgres)&#xa;&lt;font style=&quot;font-size: 10px;&quot;&gt;Bookings, Services&lt;/font&gt;" style="shape=cylinder3;whiteSpace=wrap;html=1;boundedLbl=1;backgroundOutline=1;size=15;fillColor=#fff2cc;strokeColor=#d6b656;" vertex="1" parent="c_db_box">
          <mxGeometry x="710" y="40" width="130" height="80" as="geometry" />
        </mxCell>
        <mxCell id="db_pay" value="&lt;b&gt;Payment DB&lt;/b&gt;&#xa;(Postgres)&#xa;&lt;font style=&quot;font-size: 10px;&quot;&gt;Transactions, Logs&lt;/font&gt;" style="shape=cylinder3;whiteSpace=wrap;html=1;boundedLbl=1;backgroundOutline=1;size=15;fillColor=#fff2cc;strokeColor=#d6b656;" vertex="1" parent="c_db_box">
          <mxGeometry x="920" y="40" width="130" height="80" as="geometry" />
        </mxCell>

        <!-- Service to DB Connectors -->
        <mxCell id="e_s1_db" style="edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;" edge="1" parent="1" source="c_svc_auth" target="db_auth">
          <mxGeometry relative="1" as="geometry" />
        </mxCell>
        <mxCell id="e_s2_db" style="edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;" edge="1" parent="1" source="c_svc_pet" target="db_pet">
          <mxGeometry relative="1" as="geometry" />
        </mxCell>
        <mxCell id="e_s3_db" style="edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;" edge="1" parent="1" source="c_svc_room" target="db_room">
          <mxGeometry relative="1" as="geometry" />
        </mxCell>
        <mxCell id="e_s4_db" style="edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;" edge="1" parent="1" source="c_svc_booking" target="db_bkg">
          <mxGeometry relative="1" as="geometry" />
        </mxCell>
        <mxCell id="e_s5_db" style="edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;" edge="1" parent="1" source="c_svc_payment" target="db_pay">
          <mxGeometry relative="1" as="geometry" />
        </mxCell>

        <!-- External 3rd party services -->
        <mxCell id="ext_vnpay" value="&amp;laquo;external&amp;raquo;&#xa;&lt;b&gt;VNPay Payment Gateway&lt;/b&gt;" style="shape=cube;whiteSpace=wrap;html=1;boundedLbl=1;arrows=none;fillColor=#f8cecc;strokeColor=#b85450;" vertex="1" parent="1">
          <mxGeometry x="920" y="720" width="160" height="60" as="geometry" />
        </mxCell>
        <mxCell id="ext_cloud" value="&amp;laquo;external&amp;raquo;&#xa;&lt;b&gt;Cloudinary Media Storage&lt;/b&gt;" style="shape=cube;whiteSpace=wrap;html=1;boundedLbl=1;arrows=none;fillColor=#f8cecc;strokeColor=#b85450;" vertex="1" parent="1">
          <mxGeometry x="270" y="720" width="160" height="60" as="geometry" />
        </mxCell>
        <mxCell id="e_pay_ext" style="edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;strokeColor=#b85450;" edge="1" parent="1" source="c_svc_payment" target="ext_vnpay">
          <mxGeometry relative="1" as="geometry">
            <Array as="points">
              <mxPoint x="1080" y="380" />
              <mxPoint x="1080" y="750" />
            </Array>
          </mxGeometry>
        </mxCell>
        <mxCell id="e_pet_ext" style="edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;strokeColor=#b85450;" edge="1" parent="1" source="c_svc_pet" target="ext_cloud">
          <mxGeometry relative="1" as="geometry">
            <Array as="points">
              <mxPoint x="430" y="380" />
              <mxPoint x="430" y="750" />
            </Array>
          </mxGeometry>
        </mxCell>
      </root>
    </mxGraphModel>
  </diagram>

  <diagram id="vi1-deployment" name="2. Deployment Diagram (SOA)">
    <mxGraphModel dx="1200" dy="800" grid="1" gridSize="10" guides="1" tooltips="1" connect="1" arrows="1" fold="1" page="1" pageScale="1" pageWidth="1200" pageHeight="850" math="0" shadow="0">
      <root>
        <mxCell id="0" />
        <mxCell id="1" parent="0" />

        <mxCell id="d_title" value="&lt;b&gt;&lt;font style=&quot;font-size: 16px;&quot; color=&quot;#1A365D&quot;&gt;VI.1 Deployment Diagram - Pet Care Service-Oriented Infrastructure&lt;/font&gt;&lt;/b&gt;" style="text;html=1;strokeColor=none;fillColor=none;align=center;verticalAlign=middle;whiteSpace=wrap;rounded=0;" vertex="1" parent="1">
          <mxGeometry x="250" y="20" width="700" height="30" as="geometry" />
        </mxCell>

        <!-- Client Nodes -->
        <mxCell id="node_client" value="&amp;laquo;device&amp;raquo;&#xa;&lt;b&gt;User Client Browser / Mobile Device&lt;/b&gt;" style="verticalAlign=top;align=left;shape=cube;size=10;direction=south;fontStyle=0;html=1;boundedLbl=1;spacingLeft=5;fillColor=#dae8fc;strokeColor=#6c8ebf;" vertex="1" parent="1">
          <mxGeometry x="40" y="80" width="230" height="120" as="geometry" />
        </mxCell>
        <mxCell id="art_browser" value="&amp;laquo;artifact&amp;raquo;&#xa;&lt;b&gt;SPA Web App&lt;/b&gt;&#xa;(React.js Bundle)" style="html=1;shape=module;align=center;verticalAlign=middle;whiteSpace=wrap;fillColor=#ffffff;strokeColor=#6c8ebf;" vertex="1" parent="node_client">
          <mxGeometry x="30" y="45" width="170" height="50" as="geometry" />
        </mxCell>

        <!-- Cloud DMZ / Ingress -->
        <mxCell id="node_dmz" value="&amp;laquo;environment&amp;raquo;&#xa;&lt;b&gt;Cloud DMZ / Ingress Controller (HTTPS:443)&lt;/b&gt;" style="verticalAlign=top;align=left;shape=cube;size=10;direction=south;fontStyle=0;html=1;boundedLbl=1;spacingLeft=5;fillColor=#fff2cc;strokeColor=#d6b656;" vertex="1" parent="1">
          <mxGeometry x="330" y="80" width="250" height="120" as="geometry" />
        </mxCell>
        <mxCell id="art_gw" value="&amp;laquo;execution environment&amp;raquo;&#xa;&lt;b&gt;API Gateway Pod&lt;/b&gt;&#xa;(YARP / Ocelot - Reverse Proxy)" style="html=1;shape=module;align=center;verticalAlign=middle;whiteSpace=wrap;fillColor=#ffffff;strokeColor=#d6b656;" vertex="1" parent="node_dmz">
          <mxGeometry x="25" y="45" width="200" height="55" as="geometry" />
        </mxCell>

        <!-- Link Client -> Gateway -->
        <mxCell id="e_client_gw" value="&amp;laquo;HTTPS / REST&amp;raquo;" style="edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;" edge="1" parent="1" source="node_client" target="node_dmz">
          <mxGeometry relative="1" as="geometry" />
        </mxCell>

        <!-- Kubernetes Service Cluster -->
        <mxCell id="node_cluster" value="&amp;laquo;execution environment&amp;raquo;&#xa;&lt;b&gt;Kubernetes / Docker Service Cluster (Private VPC Subnet)&lt;/b&gt;" style="verticalAlign=top;align=left;shape=cube;size=10;direction=south;fontStyle=0;html=1;boundedLbl=1;spacingLeft=5;fillColor=#f5f5f5;fontColor=#333333;strokeColor=#666666;" vertex="1" parent="1">
          <mxGeometry x="40" y="260" width="1120" height="340" as="geometry" />
        </mxCell>

        <!-- Pod 1: Identity -->
        <mxCell id="pod_auth" value="&amp;laquo;pod&amp;raquo; &lt;b&gt;Identity-Service-Pod&lt;/b&gt;&#xa;(Port: 5001)" style="shape=cube;size=8;whiteSpace=wrap;html=1;boundedLbl=1;fillColor=#d5e8d4;strokeColor=#82b366;verticalAlign=top;align=left;spacingLeft=5;" vertex="1" parent="node_cluster">
          <mxGeometry x="30" y="50" width="190" height="110" as="geometry" />
        </mxCell>
        <mxCell id="art_auth_bin" value="&amp;laquo;artifact&amp;raquo;&#xa;&lt;b&gt;IdentityService.dll&lt;/b&gt;&#xa;(.NET 8 Web API)" style="html=1;shape=module;align=center;verticalAlign=middle;whiteSpace=wrap;fillColor=#ffffff;strokeColor=#82b366;" vertex="1" parent="pod_auth">
          <mxGeometry x="15" y="45" width="160" height="45" as="geometry" />
        </mxCell>

        <!-- Pod 2: Pet & Care -->
        <mxCell id="pod_pet" value="&amp;laquo;pod&amp;raquo; &lt;b&gt;Pet-Care-Service-Pod&lt;/b&gt;&#xa;(Port: 5004)" style="shape=cube;size=8;whiteSpace=wrap;html=1;boundedLbl=1;fillColor=#d5e8d4;strokeColor=#82b366;verticalAlign=top;align=left;spacingLeft=5;" vertex="1" parent="node_cluster">
          <mxGeometry x="250" y="50" width="190" height="110" as="geometry" />
        </mxCell>
        <mxCell id="art_pet_bin" value="&amp;laquo;artifact&amp;raquo;&#xa;&lt;b&gt;PetCareService.dll&lt;/b&gt;&#xa;(.NET 8 Web API)" style="html=1;shape=module;align=center;verticalAlign=middle;whiteSpace=wrap;fillColor=#ffffff;strokeColor=#82b366;" vertex="1" parent="pod_pet">
          <mxGeometry x="15" y="45" width="160" height="45" as="geometry" />
        </mxCell>

        <!-- Pod 3: Room -->
        <mxCell id="pod_room" value="&amp;laquo;pod&amp;raquo; &lt;b&gt;Room-Service-Pod&lt;/b&gt;&#xa;(Port: 5002 - Scaled x3)" style="shape=cube;size=8;whiteSpace=wrap;html=1;boundedLbl=1;fillColor=#d5e8d4;strokeColor=#82b366;verticalAlign=top;align=left;spacingLeft=5;" vertex="1" parent="node_cluster">
          <mxGeometry x="470" y="50" width="190" height="110" as="geometry" />
        </mxCell>
        <mxCell id="art_room_bin" value="&amp;laquo;artifact&amp;raquo;&#xa;&lt;b&gt;RoomService.dll&lt;/b&gt;&#xa;(.NET 8 Web API)" style="html=1;shape=module;align=center;verticalAlign=middle;whiteSpace=wrap;fillColor=#ffffff;strokeColor=#82b366;" vertex="1" parent="pod_room">
          <mxGeometry x="15" y="45" width="160" height="45" as="geometry" />
        </mxCell>

        <!-- Pod 4: Booking -->
        <mxCell id="pod_bkg" value="&amp;laquo;pod&amp;raquo; &lt;b&gt;Booking-Service-Pod&lt;/b&gt;&#xa;(Port: 5003 - Scaled x4)" style="shape=cube;size=8;whiteSpace=wrap;html=1;boundedLbl=1;fillColor=#d5e8d4;strokeColor=#82b366;verticalAlign=top;align=left;spacingLeft=5;" vertex="1" parent="node_cluster">
          <mxGeometry x="690" y="50" width="190" height="110" as="geometry" />
        </mxCell>
        <mxCell id="art_bkg_bin" value="&amp;laquo;artifact&amp;raquo;&#xa;&lt;b&gt;BookingService.dll&lt;/b&gt;&#xa;(.NET 8 Web API)" style="html=1;shape=module;align=center;verticalAlign=middle;whiteSpace=wrap;fillColor=#ffffff;strokeColor=#82b366;" vertex="1" parent="pod_bkg">
          <mxGeometry x="15" y="45" width="160" height="45" as="geometry" />
        </mxCell>

        <!-- Pod 5: Payment -->
        <mxCell id="pod_pay" value="&amp;laquo;pod&amp;raquo; &lt;b&gt;Payment-Service-Pod&lt;/b&gt;&#xa;(Port: 5005)" style="shape=cube;size=8;whiteSpace=wrap;html=1;boundedLbl=1;fillColor=#d5e8d4;strokeColor=#82b366;verticalAlign=top;align=left;spacingLeft=5;" vertex="1" parent="node_cluster">
          <mxGeometry x="910" y="50" width="190" height="110" as="geometry" />
        </mxCell>
        <mxCell id="art_pay_bin" value="&amp;laquo;artifact&amp;raquo;&#xa;&lt;b&gt;PaymentService.dll&lt;/b&gt;&#xa;(.NET 8 Web API)" style="html=1;shape=module;align=center;verticalAlign=middle;whiteSpace=wrap;fillColor=#ffffff;strokeColor=#82b366;" vertex="1" parent="pod_pay">
          <mxGeometry x="15" y="45" width="160" height="45" as="geometry" />
        </mxCell>

        <!-- Message Broker Pod -->
        <mxCell id="pod_rabbit" value="&amp;laquo;pod&amp;raquo; &lt;b&gt;RabbitMQ Message Broker Node&lt;/b&gt; (AMQP:5672)" style="shape=cube;size=8;whiteSpace=wrap;html=1;boundedLbl=1;fillColor=#e1d5e7;strokeColor=#9673a6;verticalAlign=top;align=left;spacingLeft=5;" vertex="1" parent="node_cluster">
          <mxGeometry x="360" y="200" width="410" height="90" as="geometry" />
        </mxCell>
        <mxCell id="art_mq" value="&amp;laquo;broker instance&amp;raquo;&#xa;&lt;b&gt;RabbitMQ Cluster / Exchanges &amp; Queues&lt;/b&gt;" style="html=1;shape=module;align=center;verticalAlign=middle;whiteSpace=wrap;fillColor=#ffffff;strokeColor=#9673a6;" vertex="1" parent="pod_rabbit">
          <mxGeometry x="50" y="35" width="310" height="40" as="geometry" />
        </mxCell>

        <!-- Gateway to Pod Connectors -->
        <mxCell id="e_gw_pods" style="edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;" edge="1" parent="1" source="node_dmz" target="node_cluster">
          <mxGeometry relative="1" as="geometry" />
        </mxCell>

        <!-- Database Tier Node -->
        <mxCell id="node_db_server" value="&amp;laquo;database server&amp;raquo;&#xa;&lt;b&gt;Managed Cloud Database Instance (PostgreSQL Cluster)&lt;/b&gt;" style="verticalAlign=top;align=left;shape=cube;size=10;direction=south;fontStyle=0;html=1;boundedLbl=1;spacingLeft=5;fillColor=#fff2cc;strokeColor=#d6b656;" vertex="1" parent="1">
          <mxGeometry x="40" y="650" width="1120" height="130" as="geometry" />
        </mxCell>
        <mxCell id="dep_db_auth" value="&amp;laquo;db&amp;raquo;&#xa;&lt;b&gt;Identity_DB&lt;/b&gt;" style="shape=cylinder3;whiteSpace=wrap;html=1;fillColor=#ffffff;strokeColor=#d6b656;" vertex="1" parent="node_db_server">
          <mxGeometry x="50" y="40" width="120" height="60" as="geometry" />
        </mxCell>
        <mxCell id="dep_db_pet" value="&amp;laquo;db&amp;raquo;&#xa;&lt;b&gt;PetRecord_DB&lt;/b&gt;" style="shape=cylinder3;whiteSpace=wrap;html=1;fillColor=#ffffff;strokeColor=#d6b656;" vertex="1" parent="node_db_server">
          <mxGeometry x="270" y="40" width="120" height="60" as="geometry" />
        </mxCell>
        <mxCell id="dep_db_room" value="&amp;laquo;db&amp;raquo;&#xa;&lt;b&gt;Room_DB&lt;/b&gt;" style="shape=cylinder3;whiteSpace=wrap;html=1;fillColor=#ffffff;strokeColor=#d6b656;" vertex="1" parent="node_db_server">
          <mxGeometry x="490" y="40" width="120" height="60" as="geometry" />
        </mxCell>
        <mxCell id="dep_db_bkg" value="&amp;laquo;db&amp;raquo;&#xa;&lt;b&gt;Booking_DB&lt;/b&gt;" style="shape=cylinder3;whiteSpace=wrap;html=1;fillColor=#ffffff;strokeColor=#d6b656;" vertex="1" parent="node_db_server">
          <mxGeometry x="710" y="40" width="120" height="60" as="geometry" />
        </mxCell>
        <mxCell id="dep_db_pay" value="&amp;laquo;db&amp;raquo;&#xa;&lt;b&gt;Payment_DB&lt;/b&gt;" style="shape=cylinder3;whiteSpace=wrap;html=1;fillColor=#ffffff;strokeColor=#d6b656;" vertex="1" parent="node_db_server">
          <mxGeometry x="930" y="40" width="120" height="60" as="geometry" />
        </mxCell>

        <!-- Service to DB lines -->
        <mxCell id="e_pod_db" style="edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;" edge="1" parent="1" source="node_cluster" target="node_db_server">
          <mxGeometry relative="1" as="geometry" />
        </mxCell>
      </root>
    </mxGraphModel>
  </diagram>

  <diagram id="vi1-class" name="3. Updated Class Diagram (SOA Services)">
    <mxGraphModel dx="1200" dy="800" grid="1" gridSize="10" guides="1" tooltips="1" connect="1" arrows="1" fold="1" page="1" pageScale="1" pageWidth="1200" pageHeight="850" math="0" shadow="0">
      <root>
        <mxCell id="0" />
        <mxCell id="1" parent="0" />

        <mxCell id="c_class_title" value="&lt;b&gt;&lt;font style=&quot;font-size: 16px;&quot; color=&quot;#1A365D&quot;&gt;VI.1 Updated Class &amp; Interface Diagram - Loosely Coupled Services &amp; Contracts&lt;/font&gt;&lt;/b&gt;" style="text;html=1;strokeColor=none;fillColor=none;align=center;verticalAlign=middle;whiteSpace=wrap;rounded=0;" vertex="1" parent="1">
          <mxGeometry x="250" y="20" width="700" height="30" as="geometry" />
        </mxCell>

        <!-- Booking Service Interface -->
        <mxCell id="cls_ibooking" value="&amp;laquo;interface&amp;raquo;&#xa;&lt;b&gt;IBookingService&lt;/b&gt;&#xa;--&#xa;+ CreateBookingAsync(req: CreateBookingDTO): Task&amp;lt;BookingResponseDTO&amp;gt;&#xa;+ CancelBookingAsync(bookingId: Guid, reason: string): Task&amp;lt;bool&amp;gt;&#xa;+ GetBookingDetailAsync(bookingId: Guid): Task&amp;lt;BookingDetailDTO&amp;gt;&#xa;+ CheckInPetAsync(bookingId: Guid, staffId: Guid): Task&amp;lt;bool&amp;gt;&#xa;+ CheckOutPetAsync(bookingId: Guid): Task&amp;lt;bool&amp;gt;" style="swimlane;fontStyle=0;childLayout=stackLayout;horizontal=1;startSize=26;fillColor=#dae8fc;strokeColor=#6c8ebf;horizontalStack=0;resizeParent=1;resizeParentMax=0;resizeLast=0;collapsible=1;marginBottom=0;align=left;spacingLeft=6;" vertex="1" parent="1">
          <mxGeometry x="60" y="90" width="450" height="140" as="geometry" />
        </mxCell>

        <!-- Room Service Client Interface -->
        <mxCell id="cls_iroom" value="&amp;laquo;interface&amp;raquo;&#xa;&lt;b&gt;IRoomServiceClient (gRPC / HTTP Client)&lt;/b&gt;&#xa;--&#xa;+ CheckAvailabilityAsync(roomTypeId: Guid, checkIn: DateTime, checkOut: DateTime): Task&amp;lt;bool&amp;gt;&#xa;+ ReserveRoomSlotAsync(roomId: Guid, bookingId: Guid): Task&amp;lt;ReservationResultDTO&amp;gt;&#xa;+ ReleaseRoomSlotAsync(roomId: Guid, bookingId: Guid): Task&amp;lt;bool&amp;gt;" style="swimlane;fontStyle=0;childLayout=stackLayout;horizontal=1;startSize=26;fillColor=#d5e8d4;strokeColor=#82b366;horizontalStack=0;resizeParent=1;resizeParentMax=0;resizeLast=0;collapsible=1;marginBottom=0;align=left;spacingLeft=6;" vertex="1" parent="1">
          <mxGeometry x="630" y="90" width="510" height="110" as="geometry" />
        </mxCell>

        <!-- Payment Service Client Interface -->
        <mxCell id="cls_ipayment" value="&amp;laquo;interface&amp;raquo;&#xa;&lt;b&gt;IPaymentServiceClient (gRPC / REST Client)&lt;/b&gt;&#xa;--&#xa;+ GenerateVnPayUrlAsync(orderId: Guid, amount: decimal, ipAddress: string): Task&amp;lt;string&amp;gt;&#xa;+ VerifyIpnSignatureAsync(callbackData: IDictionary&amp;lt;string, string&amp;gt;): Task&amp;lt;PaymentStatusDTO&amp;gt;&#xa;+ RefundTransactionAsync(transId: Guid, refundReason: string): Task&amp;lt;bool&amp;gt;" style="swimlane;fontStyle=0;childLayout=stackLayout;horizontal=1;startSize=26;fillColor=#d5e8d4;strokeColor=#82b366;horizontalStack=0;resizeParent=1;resizeParentMax=0;resizeLast=0;collapsible=1;marginBottom=0;align=left;spacingLeft=6;" vertex="1" parent="1">
          <mxGeometry x="630" y="240" width="510" height="110" as="geometry" />
        </mxCell>

        <!-- Event Bus Interface -->
        <mxCell id="cls_ieventbus" value="&amp;laquo;interface&amp;raquo;&#xa;&lt;b&gt;IEventBus (Message Bus Publisher / Subscriber)&lt;/b&gt;&#xa;--&#xa;+ PublishAsync&amp;lt;TEvent&amp;gt;(@event: TEvent): Task&#xa;+ Subscribe&amp;lt;TEvent, THandler&amp;gt;(): void" style="swimlane;fontStyle=0;childLayout=stackLayout;horizontal=1;startSize=26;fillColor=#e1d5e7;strokeColor=#9673a6;horizontalStack=0;resizeParent=1;resizeParentMax=0;resizeLast=0;collapsible=1;marginBottom=0;align=left;spacingLeft=6;" vertex="1" parent="1">
          <mxGeometry x="60" y="290" width="450" height="90" as="geometry" />
        </mxCell>

        <!-- Event Contracts -->
        <mxCell id="cls_events" value="&amp;laquo;DTO / Integration Event&amp;raquo;&#xa;&lt;b&gt;BookingCreatedIntegrationEvent&lt;/b&gt;&#xa;--&#xa;+ BookingId: Guid&#xa;+ CustomerId: Guid&#xa;+ CustomerEmail: string&#xa;+ PetId: Guid&#xa;+ TotalAmount: decimal&#xa;+ CheckInDate: DateTime&#xa;+ CheckOutDate: DateTime" style="swimlane;fontStyle=0;childLayout=stackLayout;horizontal=1;startSize=26;fillColor=#fff2cc;strokeColor=#d6b656;horizontalStack=0;resizeParent=1;resizeParentMax=0;resizeLast=0;collapsible=1;marginBottom=0;align=left;spacingLeft=6;" vertex="1" parent="1">
          <mxGeometry x="60" y="440" width="260" height="150" as="geometry" />
        </mxCell>

        <mxCell id="cls_event_pay" value="&amp;laquo;DTO / Integration Event&amp;raquo;&#xa;&lt;b&gt;PaymentCompletedIntegrationEvent&lt;/b&gt;&#xa;--&#xa;+ TransactionId: Guid&#xa;+ BookingId: Guid&#xa;+ AmountPaid: decimal&#xa;+ PaymentMethod: string (VNPay)&#xa;+ Status: PaymentStatusEnum" style="swimlane;fontStyle=0;childLayout=stackLayout;horizontal=1;startSize=26;fillColor=#fff2cc;strokeColor=#d6b656;horizontalStack=0;resizeParent=1;resizeParentMax=0;resizeLast=0;collapsible=1;marginBottom=0;align=left;spacingLeft=6;" vertex="1" parent="1">
          <mxGeometry x="380" y="440" width="260" height="150" as="geometry" />
        </mxCell>

        <!-- Notification Consumer -->
        <mxCell id="cls_notif_consumer" value="&amp;laquo;integration handler&amp;raquo;&#xa;&lt;b&gt;SendEmailOnBookingConfirmedHandler&lt;/b&gt;&#xa;--&#xa;+ HandleAsync(@event: PaymentCompletedIntegrationEvent): Task" style="swimlane;fontStyle=0;childLayout=stackLayout;horizontal=1;startSize=26;fillColor=#f8cecc;strokeColor=#b85450;horizontalStack=0;resizeParent=1;resizeParentMax=0;resizeLast=0;collapsible=1;marginBottom=0;align=left;spacingLeft=6;" vertex="1" parent="1">
          <mxGeometry x="710" y="440" width="410" height="70" as="geometry" />
        </mxCell>

        <!-- Dependencies -->
        <mxCell id="e_cls_1" value="uses" style="edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;dashed=1;endArrow=open;" edge="1" parent="1" source="cls_ibooking" target="cls_iroom">
          <mxGeometry relative="1" as="geometry" />
        </mxCell>
        <mxCell id="e_cls_2" value="uses" style="edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;dashed=1;endArrow=open;" edge="1" parent="1" source="cls_ibooking" target="cls_ipayment">
          <mxGeometry relative="1" as="geometry" />
        </mxCell>
        <mxCell id="e_cls_3" value="publishes events via" style="edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;dashed=1;endArrow=open;" edge="1" parent="1" source="cls_ibooking" target="cls_ieventbus">
          <mxGeometry relative="1" as="geometry" />
        </mxCell>
        <mxCell id="e_cls_4" value="consumes" style="edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;dashed=1;endArrow=open;" edge="1" parent="1" source="cls_notif_consumer" target="cls_event_pay">
          <mxGeometry relative="1" as="geometry" />
        </mxCell>
      </root>
    </mxGraphModel>
  </diagram>
</mxfile>'''

    # File 2: VI.2 - Service Discovery Diagrams
    xml_vi2 = '''<mxfile host="app.diagrams.net" version="24.0.0" type="device">
  <diagram id="vi2-deployment" name="1. Deployment (Service Discovery & Multi-Instance)">
    <mxGraphModel dx="1200" dy="800" grid="1" gridSize="10" guides="1" tooltips="1" connect="1" arrows="1" fold="1" page="1" pageScale="1" pageWidth="1200" pageHeight="850" math="0" shadow="0">
      <root>
        <mxCell id="0" />
        <mxCell id="1" parent="0" />

        <mxCell id="sd_title" value="&lt;b&gt;&lt;font style=&quot;font-size: 16px;&quot; color=&quot;#1A365D&quot;&gt;VI.2 Deployment Diagram - Service Discovery Registry with Multi-Instance Scaling&lt;/font&gt;&lt;/b&gt;" style="text;html=1;strokeColor=none;fillColor=none;align=center;verticalAlign=middle;whiteSpace=wrap;rounded=0;" vertex="1" parent="1">
          <mxGeometry x="250" y="20" width="700" height="30" as="geometry" />
        </mxCell>

        <!-- Gateway Node -->
        <mxCell id="sd_gw_node" value="&amp;laquo;device / node&amp;raquo;&#xa;&lt;b&gt;API Gateway Host&lt;/b&gt;&#xa;[YARP / Ocelot / Envoy Proxy]" style="shape=cube;size=10;whiteSpace=wrap;html=1;boundedLbl=1;fillColor=#ffe6cc;strokeColor=#d79b00;verticalAlign=top;align=left;spacingLeft=5;" vertex="1" parent="1">
          <mxGeometry x="50" y="90" width="260" height="150" as="geometry" />
        </mxCell>
        <mxCell id="sd_gw_comp" value="&amp;laquo;component&amp;raquo;&#xa;&lt;b&gt;API Gateway Controller&lt;/b&gt;&#xa;• Reverse Proxy&#xa;• Dynamic Router&#xa;• Client-Side Load Balancer" style="html=1;shape=module;align=left;verticalAlign=middle;whiteSpace=wrap;fillColor=#ffffff;strokeColor=#d79b00;spacingLeft=8;" vertex="1" parent="sd_gw_node">
          <mxGeometry x="20" y="45" width="220" height="85" as="geometry" />
        </mxCell>

        <!-- Service Registry Cluster (Consul) -->
        <mxCell id="sd_consul_node" value="&amp;laquo;cluster&amp;raquo;&#xa;&lt;b&gt;Service Discovery Cluster&lt;/b&gt;&#xa;[HashiCorp Consul / Eureka / K8s DNS]" style="shape=cube;size=10;whiteSpace=wrap;html=1;boundedLbl=1;fillColor=#e1d5e7;strokeColor=#9673a6;verticalAlign=top;align=left;spacingLeft=5;" vertex="1" parent="1">
          <mxGeometry x="450" y="90" width="460" height="150" as="geometry" />
        </mxCell>
        <mxCell id="sd_consul_catalog" value="&amp;laquo;catalog &amp; registry&amp;raquo;&#xa;&lt;b&gt;Consul Service Catalog &amp; Health Registry&lt;/b&gt;&#xa;&lt;font style=&quot;font-size: 10px;&quot;&gt;• booking-service: [10.0.1.10:5003 (UP), 10.0.1.11:5003 (UP), 10.0.1.12:5003 (UP)]&#xa;• room-service: [10.0.1.20:5002 (UP), 10.0.1.21:5002 (UP)]&#xa;• payment-service: [10.0.1.30:5005 (UP)]&lt;/font&gt;" style="html=1;shape=module;align=left;verticalAlign=middle;whiteSpace=wrap;fillColor=#ffffff;strokeColor=#9673a6;spacingLeft=8;" vertex="1" parent="sd_consul_node">
          <mxGeometry x="20" y="45" width="420" height="85" as="geometry" />
        </mxCell>

        <!-- Link Gateway to Consul -->
        <mxCell id="e_gw_consul" value="1. Query Service Endpoints&#xa;(Watch Service Changes)" style="edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;fontStyle=1;strokeColor=#9673a6;strokeWidth=2;" edge="1" parent="1" source="sd_gw_node" target="sd_consul_node">
          <mxGeometry relative="1" as="geometry" />
        </mxCell>

        <!-- Auto-scaled Instances Container -->
        <mxCell id="sd_instances_node" value="&amp;laquo;compute cluster&amp;raquo;&#xa;&lt;b&gt;Containerized Workloads (Elastic Autoscaling Node Pool)&lt;/b&gt;" style="verticalAlign=top;align=left;shape=cube;size=10;direction=south;fontStyle=0;html=1;boundedLbl=1;spacingLeft=5;fillColor=#f5f5f5;fontColor=#333333;strokeColor=#666666;" vertex="1" parent="1">
          <mxGeometry x="50" y="310" width="1080" height="490" as="geometry" />
        </mxCell>

        <!-- Booking Service Instances (Scaled x3) -->
        <mxCell id="bkg_inst1" value="&amp;laquo;instance #1&amp;raquo;&#xa;&lt;b&gt;BookingService Pod 1&lt;/b&gt;&#xa;IP: 10.0.1.10:5003&#xa;Health: /healthz (200 OK)" style="shape=cube;size=8;whiteSpace=wrap;html=1;boundedLbl=1;fillColor=#d5e8d4;strokeColor=#82b366;" vertex="1" parent="sd_instances_node">
          <mxGeometry x="30" y="50" width="220" height="90" as="geometry" />
        </mxCell>
        <mxCell id="bkg_inst2" value="&amp;laquo;instance #2&amp;raquo;&#xa;&lt;b&gt;BookingService Pod 2&lt;/b&gt;&#xa;IP: 10.0.1.11:5003&#xa;Health: /healthz (200 OK)" style="shape=cube;size=8;whiteSpace=wrap;html=1;boundedLbl=1;fillColor=#d5e8d4;strokeColor=#82b366;" vertex="1" parent="sd_instances_node">
          <mxGeometry x="280" y="50" width="220" height="90" as="geometry" />
        </mxCell>
        <mxCell id="bkg_inst3" value="&amp;laquo;instance #3 (Autoscaled)&amp;raquo;&#xa;&lt;b&gt;BookingService Pod 3&lt;/b&gt;&#xa;IP: 10.0.1.12:5003&#xa;Health: /healthz (200 OK)" style="shape=cube;size=8;whiteSpace=wrap;html=1;boundedLbl=1;fillColor=#d5e8d4;strokeColor=#82b366;" vertex="1" parent="sd_instances_node">
          <mxGeometry x="530" y="50" width="220" height="90" as="geometry" />
        </mxCell>

        <!-- Room Service Instances (Scaled x2) -->
        <mxCell id="room_inst1" value="&amp;laquo;instance #1&amp;raquo;&#xa;&lt;b&gt;RoomService Pod 1&lt;/b&gt;&#xa;IP: 10.0.1.20:5002&#xa;Health: /healthz (200 OK)" style="shape=cube;size=8;whiteSpace=wrap;html=1;boundedLbl=1;fillColor=#dae8fc;strokeColor=#6c8ebf;" vertex="1" parent="sd_instances_node">
          <mxGeometry x="30" y="190" width="220" height="90" as="geometry" />
        </mxCell>
        <mxCell id="room_inst2" value="&amp;laquo;instance #2&amp;raquo;&#xa;&lt;b&gt;RoomService Pod 2&lt;/b&gt;&#xa;IP: 10.0.1.21:5002&#xa;Health: /healthz (200 OK)" style="shape=cube;size=8;whiteSpace=wrap;html=1;boundedLbl=1;fillColor=#dae8fc;strokeColor=#6c8ebf;" vertex="1" parent="sd_instances_node">
          <mxGeometry x="280" y="190" width="220" height="90" as="geometry" />
        </mxCell>

        <!-- Payment Service Instance -->
        <mxCell id="pay_inst1" value="&amp;laquo;instance #1&amp;raquo;&#xa;&lt;b&gt;PaymentService Pod 1&lt;/b&gt;&#xa;IP: 10.0.1.30:5005&#xa;Health: /healthz (200 OK)" style="shape=cube;size=8;whiteSpace=wrap;html=1;boundedLbl=1;fillColor=#fff2cc;strokeColor=#d6b656;" vertex="1" parent="sd_instances_node">
          <mxGeometry x="30" y="330" width="220" height="90" as="geometry" />
        </mxCell>

        <!-- Heartbeat / Registration Connectors -->
        <mxCell id="e_reg_bkg" value="Auto-Register &amp; Periodic Heartbeat (/healthz)" style="edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;dashed=1;strokeColor=#82b366;" edge="1" parent="1" source="bkg_inst3" target="sd_consul_catalog">
          <mxGeometry relative="1" as="geometry">
            <Array as="points">
              <mxPoint x="980" y="405" />
              <mxPoint x="980" y="180" />
            </Array>
          </mxGeometry>
        </mxCell>

        <!-- Gateway Direct Load-balanced Routing -->
        <mxCell id="e_gw_route" value="2. Round-Robin Load-Balanced Forwarding&#xa;http://10.0.1.11:5003/api/bookings" style="edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;strokeColor=#d79b00;strokeWidth=2;" edge="1" parent="1" source="sd_gw_comp" target="bkg_inst2">
          <mxGeometry relative="1" as="geometry">
            <Array as="points">
              <mxPoint x="180" y="390" />
              <mxPoint x="440" y="390" />
            </Array>
          </mxGeometry>
        </mxCell>
      </root>
    </mxGraphModel>
  </diagram>

  <diagram id="vi2-component" name="2. Component Diagram with Discovery Service">
    <mxGraphModel dx="1200" dy="800" grid="1" gridSize="10" guides="1" tooltips="1" connect="1" arrows="1" fold="1" page="1" pageScale="1" pageWidth="1200" pageHeight="850" math="0" shadow="0">
      <root>
        <mxCell id="0" />
        <mxCell id="1" parent="0" />

        <mxCell id="sd2_title" value="&lt;b&gt;&lt;font style=&quot;font-size: 16px;&quot; color=&quot;#1A365D&quot;&gt;VI.2 Component Interaction Diagram - Service Discovery &amp; Dynamic Registration&lt;/font&gt;&lt;/b&gt;" style="text;html=1;strokeColor=none;fillColor=none;align=center;verticalAlign=middle;whiteSpace=wrap;rounded=0;" vertex="1" parent="1">
          <mxGeometry x="250" y="20" width="700" height="30" as="geometry" />
        </mxCell>

        <!-- Service Discovery Component -->
        <mxCell id="sd_center" value="&amp;laquo;infrastructure component&amp;raquo;&#xa;&lt;b&gt;Service Discovery Server (Consul / Eureka)&lt;/b&gt;&#xa;--&#xa;+ RegisterInstance(meta: ServiceRegistrationDTO): void&#xa;+ DeregisterInstance(instanceId: string): void&#xa;+ DiscoverServices(serviceName: string): List&amp;lt;ServiceInstanceDTO&amp;gt;&#xa;+ HealthCheckMonitor(): void" style="swimlane;fontStyle=0;childLayout=stackLayout;horizontal=1;startSize=26;fillColor=#e1d5e7;strokeColor=#9673a6;horizontalStack=0;resizeParent=1;resizeParentMax=0;resizeLast=0;collapsible=1;marginBottom=0;align=left;spacingLeft=6;" vertex="1" parent="1">
          <mxGeometry x="380" y="80" width="440" height="130" as="geometry" />
        </mxCell>

        <!-- Gateway Component -->
        <mxCell id="comp_gateway" value="&amp;laquo;component&amp;raquo;&#xa;&lt;b&gt;Dynamic API Gateway&lt;/b&gt;&#xa;--&#xa;+ ResolveTargetEndpoint(logicalRoute: string): Uri&#xa;+ ApplyLoadBalancing(candidates: List&amp;lt;Uri&amp;gt;): Uri&#xa;+ ProxyRequestAsync(request: HttpRequest): Task&amp;lt;HttpResponse&amp;gt;" style="swimlane;fontStyle=0;childLayout=stackLayout;horizontal=1;startSize=26;fillColor=#ffe6cc;strokeColor=#d79b00;horizontalStack=0;resizeParent=1;resizeParentMax=0;resizeLast=0;collapsible=1;marginBottom=0;align=left;spacingLeft=6;" vertex="1" parent="1">
          <mxGeometry x="50" y="280" width="360" height="110" as="geometry" />
        </mxCell>

        <!-- Microservice with Self-Registration Component -->
        <mxCell id="comp_svc_bkg" value="&amp;laquo;microservice component&amp;raquo;&#xa;&lt;b&gt;BookingService Instance (Self-Registering)&lt;/b&gt;&#xa;--&#xa;+ OnApplicationStarted(): RegisterToDiscovery()&#xa;+ OnApplicationStopping(): DeregisterFromDiscovery()&#xa;+ HealthCheckEndpoint(): 200 OK /healthz&#xa;+ ProcessBookingAsync(dto: CreateBookingDTO): Task" style="swimlane;fontStyle=0;childLayout=stackLayout;horizontal=1;startSize=26;fillColor=#d5e8d4;strokeColor=#82b366;horizontalStack=0;resizeParent=1;resizeParentMax=0;resizeLast=0;collapsible=1;marginBottom=0;align=left;spacingLeft=6;" vertex="1" parent="1">
          <mxGeometry x="500" y="280" width="370" height="120" as="geometry" />
        </mxCell>

        <!-- RoomService Instance Component -->
        <mxCell id="comp_svc_room" value="&amp;laquo;microservice component&amp;raquo;&#xa;&lt;b&gt;RoomService Instance (Self-Registering)&lt;/b&gt;&#xa;--&#xa;+ OnApplicationStarted(): RegisterToDiscovery()&#xa;+ HealthCheckEndpoint(): 200 OK /healthz&#xa;+ CheckRoomAvailability(dateRange: DateRangeDTO): Task" style="swimlane;fontStyle=0;childLayout=stackLayout;horizontal=1;startSize=26;fillColor=#dae8fc;strokeColor=#6c8ebf;horizontalStack=0;resizeParent=1;resizeParentMax=0;resizeLast=0;collapsible=1;marginBottom=0;align=left;spacingLeft=6;" vertex="1" parent="1">
          <mxGeometry x="500" y="460" width="370" height="110" as="geometry" />
        </mxCell>

        <!-- Connectors -->
        <mxCell id="e_conn_1" value="1. Register &amp; Heartbeat (Self-Registration)" style="edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;strokeColor=#82b366;strokeWidth=1.5;" edge="1" parent="1" source="comp_svc_bkg" target="sd_center">
          <mxGeometry relative="1" as="geometry" />
        </mxCell>
        <mxCell id="e_conn_2" value="1. Register &amp; Heartbeat" style="edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;strokeColor=#6c8ebf;strokeWidth=1.5;" edge="1" parent="1" source="comp_svc_room" target="sd_center">
          <mxGeometry relative="1" as="geometry">
            <Array as="points">
              <mxPoint x="930" y="515" />
              <mxPoint x="930" y="145" />
            </Array>
          </mxGeometry>
        </mxCell>
        <mxCell id="e_conn_3" value="2. Discover healthy instances&#xa;(Lookup: &quot;booking-service&quot;)" style="edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;strokeColor=#9673a6;strokeWidth=1.5;" edge="1" parent="1" source="comp_gateway" target="sd_center">
          <mxGeometry relative="1" as="geometry">
            <Array as="points">
              <mxPoint x="230" y="145" />
            </Array>
          </mxGeometry>
        </mxCell>
        <mxCell id="e_conn_4" value="3. Forward request with Client-Side Load Balancing" style="edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;strokeColor=#d79b00;strokeWidth=2;" edge="1" parent="1" source="comp_gateway" target="comp_svc_bkg">
          <mxGeometry relative="1" as="geometry" />
        </mxCell>
        <mxCell id="e_conn_5" value="4. Service-to-Service Discovery &amp; RPC Call" style="edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;strokeColor=#000000;strokeWidth=1.5;strokeDasharray=3 3;" edge="1" parent="1" source="comp_svc_bkg" target="comp_svc_room">
          <mxGeometry relative="1" as="geometry" />
        </mxCell>
      </root>
    </mxGraphModel>
  </diagram>
</mxfile>'''

    with open(r'd:\KI 7\SWD392\PetCareWebsite\docs\diagrams\VI_1_soa_diagrams.drawio.xml', 'w', encoding='utf-8') as f:
        f.write(xml_vi1.strip())
        
    with open(r'd:\KI 7\SWD392\PetCareWebsite\docs\diagrams\VI_2_service_discovery_diagrams.drawio.xml', 'w', encoding='utf-8') as f:
        f.write(xml_vi2.strip())

    print("Successfully generated both draw.io diagram files!")

if __name__ == '__main__':
    generate_diagrams()
