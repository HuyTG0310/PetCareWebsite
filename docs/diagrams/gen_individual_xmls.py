# -*- coding: utf-8 -*-
import html

def make_xml(content, title="Diagram"):
    return f'''<mxGraphModel dx="1200" dy="800" grid="1" gridSize="10" guides="1" tooltips="1" connect="1" arrows="1" fold="1" page="1" pageScale="1" pageWidth="1200" pageHeight="900" math="0" shadow="0">
  <root>
    <mxCell id="0" />
    <mxCell id="1" parent="0" />
{content}
  </root>
</mxGraphModel>'''

# 1. VI.1 Component Diagram
vi1_comp = '''    <!-- Title -->
    <mxCell id="title" value="&lt;b&gt;&lt;font style=&quot;font-size: 16px;&quot; color=&quot;#0f3460&quot;&gt;VI.1 Component Diagram - Pet Care Service-Oriented Architecture (SOA)&lt;/font&gt;&lt;/b&gt;" style="text;html=1;align=center;verticalAlign=middle;rounded=0;" vertex="1" parent="1">
      <mxGeometry x="250" y="20" width="700" height="30" as="geometry" />
    </mxCell>

    <!-- Client Box -->
    <mxCell id="c_client_box" value="Frontend &amp; Client Tier" style="swimlane;startSize=25;fontStyle=1;align=center;verticalAlign=top;html=1;whiteSpace=wrap;fillColor=#f8f9fa;strokeColor=#495057;strokeWidth=1.5;" vertex="1" parent="1">
      <mxGeometry x="60" y="70" width="1080" height="90" as="geometry" />
    </mxCell>
    <mxCell id="c_web" value="«component»&lt;br&gt;&lt;b&gt;Customer Web App&lt;/b&gt;&lt;br&gt;(React.js)" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#e1f5fe;strokeColor=#0288d1;strokeWidth=1.5;" vertex="1" parent="c_client_box">
      <mxGeometry x="120" y="32" width="200" height="48" as="geometry" />
    </mxCell>
    <mxCell id="c_admin" value="«component»&lt;br&gt;&lt;b&gt;Admin &amp; Staff Portal&lt;/b&gt;&lt;br&gt;(React.js / Web Admin)" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#e1f5fe;strokeColor=#0288d1;strokeWidth=1.5;" vertex="1" parent="c_client_box">
      <mxGeometry x="440" y="32" width="200" height="48" as="geometry" />
    </mxCell>
    <mxCell id="c_pos" value="«component»&lt;br&gt;&lt;b&gt;Mobile / POS App&lt;/b&gt;&lt;br&gt;(Future Channel)" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#e1f5fe;strokeColor=#0288d1;strokeWidth=1.5;strokeDasharray=3 3;" vertex="1" parent="c_client_box">
      <mxGeometry x="760" y="32" width="200" height="48" as="geometry" />
    </mxCell>

    <!-- API Gateway -->
    <mxCell id="c_gw" value="«component»&lt;br&gt;&lt;b style=&quot;font-size: 13px;&quot;&gt;API Gateway (YARP / Ocelot)&lt;/b&gt;&lt;br&gt;&lt;font style=&quot;font-size: 11px;&quot; color=&quot;#555555&quot;&gt;[Single Entry Point | JWT Validation | Reverse Proxy | Rate Limiting]&lt;/font&gt;" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#fff3e0;strokeColor=#e65100;strokeWidth=2;" vertex="1" parent="1">
      <mxGeometry x="260" y="195" width="680" height="60" as="geometry" />
    </mxCell>

    <mxCell id="e_w_gw" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;entryX=0.2;entryY=0;strokeWidth=1.5;" edge="1" parent="1" source="c_web" target="c_gw">
      <mxGeometry relative="1" as="geometry" />
    </mxCell>
    <mxCell id="e_a_gw" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;entryX=0.5;entryY=0;strokeWidth=1.5;" edge="1" parent="1" source="c_admin" target="c_gw">
      <mxGeometry relative="1" as="geometry" />
    </mxCell>
    <mxCell id="e_p_gw" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;entryX=0.8;entryY=0;strokeWidth=1.5;strokeDasharray=3 3;" edge="1" parent="1" source="c_pos" target="c_gw">
      <mxGeometry relative="1" as="geometry" />
    </mxCell>

    <!-- Services Container -->
    <mxCell id="c_svc_box" value="Autonomous Business Services Layer (SOA)" style="swimlane;startSize=25;fontStyle=1;align=center;verticalAlign=top;html=1;whiteSpace=wrap;fillColor=#f8f9fa;strokeColor=#495057;strokeWidth=1.5;" vertex="1" parent="1">
      <mxGeometry x="60" y="290" width="1080" height="230" as="geometry" />
    </mxCell>

    <mxCell id="s_auth" value="«service»&lt;br&gt;&lt;b&gt;Identity &amp; User Service&lt;/b&gt;&lt;br&gt;&lt;font style=&quot;font-size: 10px;&quot; color=&quot;#444444&quot;&gt;• AuthController, JWT&lt;br&gt;• Customer, Staff, Roles&lt;/font&gt;" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#e8f5e9;strokeColor=#2e7d32;strokeWidth=1.5;" vertex="1" parent="c_svc_box">
      <mxGeometry x="20" y="40" width="190" height="75" as="geometry" />
    </mxCell>

    <mxCell id="s_pet" value="«service»&lt;br&gt;&lt;b&gt;Pet &amp; Care Record Service&lt;/b&gt;&lt;br&gt;&lt;font style=&quot;font-size: 10px;&quot; color=&quot;#444444&quot;&gt;• Pets, Health Profiles&lt;br&gt;• CareRecords (Daily Logs)&lt;/font&gt;" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#e8f5e9;strokeColor=#2e7d32;strokeWidth=1.5;" vertex="1" parent="c_svc_box">
      <mxGeometry x="235" y="40" width="190" height="75" as="geometry" />
    </mxCell>

    <mxCell id="s_room" value="«service»&lt;br&gt;&lt;b&gt;Room &amp; Facility Service&lt;/b&gt;&lt;br&gt;&lt;font style=&quot;font-size: 10px;&quot; color=&quot;#444444&quot;&gt;• Rooms &amp; RoomTypes&lt;br&gt;• Real-time Availability&lt;/font&gt;" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#e8f5e9;strokeColor=#2e7d32;strokeWidth=1.5;" vertex="1" parent="c_svc_box">
      <mxGeometry x="445" y="40" width="190" height="75" as="geometry" />
    </mxCell>

    <mxCell id="s_bkg" value="«service»&lt;br&gt;&lt;b&gt;Booking &amp; Order Service&lt;/b&gt;&lt;br&gt;&lt;font style=&quot;font-size: 10px;&quot; color=&quot;#444444&quot;&gt;• Bookings &amp; Items&lt;br&gt;• Services, Vouchers&lt;/font&gt;" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#e8f5e9;strokeColor=#2e7d32;strokeWidth=1.5;" vertex="1" parent="c_svc_box">
      <mxGeometry x="655" y="40" width="190" height="75" as="geometry" />
    </mxCell>

    <mxCell id="s_pay" value="«service»&lt;br&gt;&lt;b&gt;Payment &amp; Notify Service&lt;/b&gt;&lt;br&gt;&lt;font style=&quot;font-size: 10px;&quot; color=&quot;#444444&quot;&gt;• Payments, Transactions&lt;br&gt;• VNPay &amp; EmailService&lt;/font&gt;" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#e8f5e9;strokeColor=#2e7d32;strokeWidth=1.5;" vertex="1" parent="c_svc_box">
      <mxGeometry x="870" y="40" width="190" height="75" as="geometry" />
    </mxCell>

    <!-- Message Broker Event Bus -->
    <mxCell id="s_bus" value="«event bus»&lt;br&gt;&lt;b&gt;Message Broker (RabbitMQ / Azure Service Bus)&lt;/b&gt;&lt;br&gt;&lt;font style=&quot;font-size: 10px;&quot; color=&quot;#444444&quot;&gt;[Events: BookingCreatedEvent | PaymentCompletedEvent | CareLogUpdatedEvent]&lt;/font&gt;" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#f3e5f5;strokeColor=#7b1fa2;strokeWidth=1.5;" vertex="1" parent="c_svc_box">
      <mxGeometry x="235" y="150" width="610" height="55" as="geometry" />
    </mxCell>

    <!-- Async Arrows -->
    <mxCell id="e_bkg_bus" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;entryX=0.85;entryY=0;strokeColor=#7b1fa2;strokeWidth=1.5;strokeDasharray=4 4;" edge="1" parent="c_svc_box" source="s_bkg" target="s_bus">
      <mxGeometry relative="1" as="geometry" />
    </mxCell>
    <mxCell id="e_bus_pay" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;entryX=0.3;entryY=1;strokeColor=#7b1fa2;strokeWidth=1.5;strokeDasharray=4 4;" edge="1" parent="c_svc_box" source="s_bus" target="s_pay">
      <mxGeometry relative="1" as="geometry" />
    </mxCell>
    <mxCell id="e_bus_pet" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;entryX=0.5;entryY=1;strokeColor=#7b1fa2;strokeWidth=1.5;strokeDasharray=4 4;" edge="1" parent="c_svc_box" source="s_bus" target="s_pet">
      <mxGeometry relative="1" as="geometry" />
    </mxCell>

    <!-- Routing arrows Gateway to Services -->
    <mxCell id="e_gw_s1" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;entryX=0.5;entryY=0;strokeWidth=1.5;" edge="1" parent="1" source="c_gw" target="s_auth">
      <mxGeometry relative="1" as="geometry" />
    </mxCell>
    <mxCell id="e_gw_s2" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;entryX=0.5;entryY=0;strokeWidth=1.5;" edge="1" parent="1" source="c_gw" target="s_pet">
      <mxGeometry relative="1" as="geometry" />
    </mxCell>
    <mxCell id="e_gw_s3" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;entryX=0.5;entryY=0;strokeWidth=1.5;" edge="1" parent="1" source="c_gw" target="s_room">
      <mxGeometry relative="1" as="geometry" />
    </mxCell>
    <mxCell id="e_gw_s4" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;entryX=0.5;entryY=0;strokeWidth=1.5;" edge="1" parent="1" source="c_gw" target="s_bkg">
      <mxGeometry relative="1" as="geometry" />
    </mxCell>
    <mxCell id="e_gw_s5" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;entryX=0.5;entryY=0;strokeWidth=1.5;" edge="1" parent="1" source="c_gw" target="s_pay">
      <mxGeometry relative="1" as="geometry" />
    </mxCell>

    <!-- DB Layer -->
    <mxCell id="c_db_box" value="Database Tier (Database-per-Service Pattern)" style="swimlane;startSize=25;fontStyle=1;align=center;verticalAlign=top;html=1;whiteSpace=wrap;fillColor=#f8f9fa;strokeColor=#495057;strokeWidth=1.5;" vertex="1" parent="1">
      <mxGeometry x="60" y="550" width="1080" height="130" as="geometry" />
    </mxCell>
    <mxCell id="db_auth" value="&lt;b&gt;Identity DB&lt;/b&gt;&lt;br&gt;&lt;font style=&quot;font-size: 10px;&quot; color=&quot;#555555&quot;&gt;(Users, Roles)&lt;/font&gt;" style="shape=cylinder3;whiteSpace=wrap;html=1;boundedLbl=1;size=10;fillColor=#fff8e1;strokeColor=#f57f17;" vertex="1" parent="c_db_box">
      <mxGeometry x="40" y="38" width="150" height="70" as="geometry" />
    </mxCell>
    <mxCell id="db_pet" value="&lt;b&gt;Pet &amp; Care DB&lt;/b&gt;&lt;br&gt;&lt;font style=&quot;font-size: 10px;&quot; color=&quot;#555555&quot;&gt;(Pets, Records)&lt;/font&gt;" style="shape=cylinder3;whiteSpace=wrap;html=1;boundedLbl=1;size=10;fillColor=#fff8e1;strokeColor=#f57f17;" vertex="1" parent="c_db_box">
      <mxGeometry x="255" y="38" width="150" height="70" as="geometry" />
    </mxCell>
    <mxCell id="db_room" value="&lt;b&gt;Room DB&lt;/b&gt;&lt;br&gt;&lt;font style=&quot;font-size: 10px;&quot; color=&quot;#555555&quot;&gt;(Rooms, Types)&lt;/font&gt;" style="shape=cylinder3;whiteSpace=wrap;html=1;boundedLbl=1;size=10;fillColor=#fff8e1;strokeColor=#f57f17;" vertex="1" parent="c_db_box">
      <mxGeometry x="465" y="38" width="150" height="70" as="geometry" />
    </mxCell>
    <mxCell id="db_bkg" value="&lt;b&gt;Booking DB&lt;/b&gt;&lt;br&gt;&lt;font style=&quot;font-size: 10px;&quot; color=&quot;#555555&quot;&gt;(Bookings, Items)&lt;/font&gt;" style="shape=cylinder3;whiteSpace=wrap;html=1;boundedLbl=1;size=10;fillColor=#fff8e1;strokeColor=#f57f17;" vertex="1" parent="c_db_box">
      <mxGeometry x="675" y="38" width="150" height="70" as="geometry" />
    </mxCell>
    <mxCell id="db_pay" value="&lt;b&gt;Payment DB&lt;/b&gt;&lt;br&gt;&lt;font style=&quot;font-size: 10px;&quot; color=&quot;#555555&quot;&gt;(Transactions)&lt;/font&gt;" style="shape=cylinder3;whiteSpace=wrap;html=1;boundedLbl=1;size=10;fillColor=#fff8e1;strokeColor=#f57f17;" vertex="1" parent="c_db_box">
      <mxGeometry x="890" y="38" width="150" height="70" as="geometry" />
    </mxCell>

    <mxCell id="e_s1_d" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;strokeWidth=1.5;" edge="1" parent="1" source="s_auth" target="db_auth">
      <mxGeometry relative="1" as="geometry" />
    </mxCell>
    <mxCell id="e_s2_d" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;strokeWidth=1.5;" edge="1" parent="1" source="s_pet" target="db_pet">
      <mxGeometry relative="1" as="geometry" />
    </mxCell>
    <mxCell id="e_s3_d" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;strokeWidth=1.5;" edge="1" parent="1" source="s_room" target="db_room">
      <mxGeometry relative="1" as="geometry" />
    </mxCell>
    <mxCell id="e_s4_d" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;strokeWidth=1.5;" edge="1" parent="1" source="s_bkg" target="db_bkg">
      <mxGeometry relative="1" as="geometry" />
    </mxCell>
    <mxCell id="e_s5_d" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;strokeWidth=1.5;" edge="1" parent="1" source="s_pay" target="db_pay">
      <mxGeometry relative="1" as="geometry" />
    </mxCell>'''

# 2. VI.1 Deployment Diagram
vi1_deploy = '''    <!-- Title -->
    <mxCell id="d_title" value="&lt;b&gt;&lt;font style=&quot;font-size: 16px;&quot; color=&quot;#0f3460&quot;&gt;VI.1 Deployment Diagram - Pet Care Service-Oriented Infrastructure&lt;/font&gt;&lt;/b&gt;" style="text;html=1;align=center;verticalAlign=middle;rounded=0;" vertex="1" parent="1">
      <mxGeometry x="250" y="20" width="700" height="30" as="geometry" />
    </mxCell>

    <!-- Client Device -->
    <mxCell id="n_client" value="«device»&lt;br&gt;&lt;b&gt;Client Web Browser / Mobile&lt;/b&gt;" style="shape=cube;size=10;whiteSpace=wrap;html=1;boundedLbl=1;fillColor=#e1f5fe;strokeColor=#0288d1;verticalAlign=top;align=center;spacingTop=20;" vertex="1" parent="1">
      <mxGeometry x="60" y="80" width="220" height="110" as="geometry" />
    </mxCell>
    <mxCell id="a_browser" value="«artifact»&lt;br&gt;&lt;b&gt;Web Single Page App&lt;/b&gt;&lt;br&gt;(React.js Bundle)" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#ffffff;strokeColor=#0288d1;" vertex="1" parent="n_client">
      <mxGeometry x="25" y="45" width="170" height="48" as="geometry" />
    </mxCell>

    <!-- DMZ Gateway -->
    <mxCell id="n_dmz" value="«environment»&lt;br&gt;&lt;b&gt;Cloud Ingress &amp; API Gateway&lt;/b&gt;" style="shape=cube;size=10;whiteSpace=wrap;html=1;boundedLbl=1;fillColor=#fff3e0;strokeColor=#e65100;verticalAlign=top;align=center;spacingTop=20;" vertex="1" parent="1">
      <mxGeometry x="380" y="80" width="260" height="110" as="geometry" />
    </mxCell>
    <mxCell id="a_gw" value="«execution env»&lt;br&gt;&lt;b&gt;YARP / Ocelot Gateway Pod&lt;/b&gt;&lt;br&gt;(Reverse Proxy :443)" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#ffffff;strokeColor=#e65100;" vertex="1" parent="n_dmz">
      <mxGeometry x="25" y="45" width="210" height="48" as="geometry" />
    </mxCell>

    <mxCell id="e_c_gw" value="HTTPS / REST" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;strokeWidth=1.5;fontStyle=1;" edge="1" parent="1" source="n_client" target="n_dmz">
      <mxGeometry relative="1" as="geometry" />
    </mxCell>

    <!-- K8s Cluster -->
    <mxCell id="n_k8s" value="«execution environment»  Kubernetes / Docker Service Cluster (Private VPC)" style="swimlane;startSize=25;fontStyle=1;align=center;verticalAlign=top;html=1;whiteSpace=wrap;fillColor=#f8f9fa;strokeColor=#495057;strokeWidth=1.5;" vertex="1" parent="1">
      <mxGeometry x="60" y="240" width="1080" height="340" as="geometry" />
    </mxCell>

    <mxCell id="pod_auth" value="«pod»&lt;br&gt;&lt;b&gt;Identity-Service&lt;/b&gt;&lt;br&gt;(:5001)" style="shape=cube;size=8;whiteSpace=wrap;html=1;fillColor=#e8f5e9;strokeColor=#2e7d32;verticalAlign=top;align=center;spacingTop=16;" vertex="1" parent="n_k8s">
      <mxGeometry x="20" y="45" width="190" height="100" as="geometry" />
    </mxCell>
    <mxCell id="bin_auth" value="IdentityService.dll&lt;br&gt;(.NET 8 Web API)" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#ffffff;strokeColor=#2e7d32;" vertex="1" parent="pod_auth">
      <mxGeometry x="20" y="45" width="150" height="40" as="geometry" />
    </mxCell>

    <mxCell id="pod_pet" value="«pod»&lt;br&gt;&lt;b&gt;PetCare-Service&lt;/b&gt;&lt;br&gt;(:5004)" style="shape=cube;size=8;whiteSpace=wrap;html=1;fillColor=#e8f5e9;strokeColor=#2e7d32;verticalAlign=top;align=center;spacingTop=16;" vertex="1" parent="n_k8s">
      <mxGeometry x="235" y="45" width="190" height="100" as="geometry" />
    </mxCell>
    <mxCell id="bin_pet" value="PetCareService.dll&lt;br&gt;(.NET 8 Web API)" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#ffffff;strokeColor=#2e7d32;" vertex="1" parent="pod_pet">
      <mxGeometry x="20" y="45" width="150" height="40" as="geometry" />
    </mxCell>

    <mxCell id="pod_room" value="«pod»&lt;br&gt;&lt;b&gt;Room-Service (x3)&lt;/b&gt;&lt;br&gt;(:5002 - Scaled)" style="shape=cube;size=8;whiteSpace=wrap;html=1;fillColor=#e8f5e9;strokeColor=#2e7d32;verticalAlign=top;align=center;spacingTop=16;" vertex="1" parent="n_k8s">
      <mxGeometry x="445" y="45" width="190" height="100" as="geometry" />
    </mxCell>
    <mxCell id="bin_room" value="RoomService.dll&lt;br&gt;(.NET 8 Web API)" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#ffffff;strokeColor=#2e7d32;" vertex="1" parent="pod_room">
      <mxGeometry x="20" y="45" width="150" height="40" as="geometry" />
    </mxCell>

    <mxCell id="pod_bkg" value="«pod»&lt;br&gt;&lt;b&gt;Booking-Service (x4)&lt;/b&gt;&lt;br&gt;(:5003 - Scaled)" style="shape=cube;size=8;whiteSpace=wrap;html=1;fillColor=#e8f5e9;strokeColor=#2e7d32;verticalAlign=top;align=center;spacingTop=16;" vertex="1" parent="n_k8s">
      <mxGeometry x="655" y="45" width="190" height="100" as="geometry" />
    </mxCell>
    <mxCell id="bin_bkg" value="BookingService.dll&lt;br&gt;(.NET 8 Web API)" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#ffffff;strokeColor=#2e7d32;" vertex="1" parent="pod_bkg">
      <mxGeometry x="20" y="45" width="150" height="40" as="geometry" />
    </mxCell>

    <mxCell id="pod_pay" value="«pod»&lt;br&gt;&lt;b&gt;Payment-Service&lt;/b&gt;&lt;br&gt;(:5005)" style="shape=cube;size=8;whiteSpace=wrap;html=1;fillColor=#e8f5e9;strokeColor=#2e7d32;verticalAlign=top;align=center;spacingTop=16;" vertex="1" parent="n_k8s">
      <mxGeometry x="870" y="45" width="190" height="100" as="geometry" />
    </mxCell>
    <mxCell id="bin_pay" value="PaymentService.dll&lt;br&gt;(.NET 8 Web API)" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#ffffff;strokeColor=#2e7d32;" vertex="1" parent="pod_pay">
      <mxGeometry x="20" y="45" width="150" height="40" as="geometry" />
    </mxCell>

    <!-- RabbitMQ Pod -->
    <mxCell id="pod_mq" value="«pod»&lt;br&gt;&lt;b&gt;RabbitMQ Message Broker Node&lt;/b&gt; (AMQP:5672)" style="shape=cube;size=8;whiteSpace=wrap;html=1;fillColor=#f3e5f5;strokeColor=#7b1fa2;verticalAlign=top;align=center;spacingTop=16;" vertex="1" parent="n_k8s">
      <mxGeometry x="340" y="190" width="400" height="110" as="geometry" />
    </mxCell>
    <mxCell id="bin_mq" value="RabbitMQ Queue &amp; Exchange Clusters" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#ffffff;strokeColor=#7b1fa2;" vertex="1" parent="pod_mq">
      <mxGeometry x="30" y="48" width="340" height="40" as="geometry" />
    </mxCell>

    <mxCell id="e_gw_k8s" value="Internal HTTP Routing" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;strokeWidth=1.5;" edge="1" parent="1" source="n_dmz" target="n_k8s">
      <mxGeometry relative="1" as="geometry" />
    </mxCell>

    <!-- Database Nodes -->
    <mxCell id="n_db_server" value="«database server»  Managed PostgreSQL Cluster Tier" style="swimlane;startSize=25;fontStyle=1;align=center;verticalAlign=top;html=1;whiteSpace=wrap;fillColor=#f8f9fa;strokeColor=#495057;strokeWidth=1.5;" vertex="1" parent="1">
      <mxGeometry x="60" y="620" width="1080" height="130" as="geometry" />
    </mxCell>
    <mxCell id="dep_db_auth" value="Identity_DB" style="shape=cylinder3;whiteSpace=wrap;html=1;size=10;fillColor=#fff8e1;strokeColor=#f57f17;" vertex="1" parent="n_db_server">
      <mxGeometry x="40" y="40" width="150" height="65" as="geometry" />
    </mxCell>
    <mxCell id="dep_db_pet" value="PetRecord_DB" style="shape=cylinder3;whiteSpace=wrap;html=1;size=10;fillColor=#fff8e1;strokeColor=#f57f17;" vertex="1" parent="n_db_server">
      <mxGeometry x="255" y="40" width="150" height="65" as="geometry" />
    </mxCell>
    <mxCell id="dep_db_room" value="Room_DB" style="shape=cylinder3;whiteSpace=wrap;html=1;size=10;fillColor=#fff8e1;strokeColor=#f57f17;" vertex="1" parent="n_db_server">
      <mxGeometry x="465" y="40" width="150" height="65" as="geometry" />
    </mxCell>
    <mxCell id="dep_db_bkg" value="Booking_DB" style="shape=cylinder3;whiteSpace=wrap;html=1;size=10;fillColor=#fff8e1;strokeColor=#f57f17;" vertex="1" parent="n_db_server">
      <mxGeometry x="675" y="40" width="150" height="65" as="geometry" />
    </mxCell>
    <mxCell id="dep_db_pay" value="Payment_DB" style="shape=cylinder3;whiteSpace=wrap;html=1;size=10;fillColor=#fff8e1;strokeColor=#f57f17;" vertex="1" parent="n_db_server">
      <mxGeometry x="890" y="40" width="150" height="65" as="geometry" />
    </mxCell>'''

# 3. VI.1 Updated Class Diagram
vi1_class = '''    <!-- Title -->
    <mxCell id="c_title" value="&lt;b&gt;&lt;font style=&quot;font-size: 16px;&quot; color=&quot;#0f3460&quot;&gt;VI.1 Updated Class &amp; Interface Diagram - Loosely Coupled Services &amp; Contracts&lt;/font&gt;&lt;/b&gt;" style="text;html=1;align=center;verticalAlign=middle;rounded=0;" vertex="1" parent="1">
      <mxGeometry x="250" y="20" width="700" height="30" as="geometry" />
    </mxCell>

    <!-- Booking Service Interface -->
    <mxCell id="cls_ibkg" value="«interface»&lt;br&gt;&lt;b&gt;IBookingService&lt;/b&gt;&lt;hr&gt;&lt;p style=&quot;margin:4px; text-align:left;&quot;&gt;+ CreateBookingAsync(req: CreateBookingDTO): Task&amp;lt;BookingResponseDTO&amp;gt;&lt;br&gt;+ CancelBookingAsync(bookingId: Guid, reason: string): Task&amp;lt;bool&amp;gt;&lt;br&gt;+ GetBookingDetailAsync(bookingId: Guid): Task&amp;lt;BookingDetailDTO&amp;gt;&lt;br&gt;+ CheckInPetAsync(bookingId: Guid, staffId: Guid): Task&amp;lt;bool&amp;gt;&lt;br&gt;+ CheckOutPetAsync(bookingId: Guid): Task&amp;lt;bool&amp;gt;&lt;/p&gt;" style="rounded=0;whiteSpace=wrap;html=1;fillColor=#e1f5fe;strokeColor=#0288d1;strokeWidth=1.5;verticalAlign=top;spacingTop=4;" vertex="1" parent="1">
      <mxGeometry x="60" y="80" width="450" height="150" as="geometry" />
    </mxCell>

    <!-- Room Client Interface -->
    <mxCell id="cls_iroom" value="«interface»&lt;br&gt;&lt;b&gt;IRoomServiceClient (gRPC / HTTP Client)&lt;/b&gt;&lt;hr&gt;&lt;p style=&quot;margin:4px; text-align:left;&quot;&gt;+ CheckAvailabilityAsync(roomTypeId: Guid, from: DateTime, to: DateTime): Task&amp;lt;bool&amp;gt;&lt;br&gt;+ ReserveRoomSlotAsync(roomId: Guid, bookingId: Guid): Task&amp;lt;ReservationResultDTO&amp;gt;&lt;br&gt;+ ReleaseRoomSlotAsync(roomId: Guid, bookingId: Guid): Task&amp;lt;bool&amp;gt;&lt;/p&gt;" style="rounded=0;whiteSpace=wrap;html=1;fillColor=#e8f5e9;strokeColor=#2e7d32;strokeWidth=1.5;verticalAlign=top;spacingTop=4;" vertex="1" parent="1">
      <mxGeometry x="630" y="80" width="510" height="120" as="geometry" />
    </mxCell>

    <!-- Payment Client Interface -->
    <mxCell id="cls_ipay" value="«interface»&lt;br&gt;&lt;b&gt;IPaymentServiceClient (gRPC / REST Client)&lt;/b&gt;&lt;hr&gt;&lt;p style=&quot;margin:4px; text-align:left;&quot;&gt;+ GenerateVnPayUrlAsync(orderId: Guid, amount: decimal, ip: string): Task&amp;lt;string&amp;gt;&lt;br&gt;+ VerifyIpnSignatureAsync(callbackData: IDictionary): Task&amp;lt;PaymentStatusDTO&amp;gt;&lt;br&gt;+ RefundTransactionAsync(transId: Guid, reason: string): Task&amp;lt;bool&amp;gt;&lt;/p&gt;" style="rounded=0;whiteSpace=wrap;html=1;fillColor=#e8f5e9;strokeColor=#2e7d32;strokeWidth=1.5;verticalAlign=top;spacingTop=4;" vertex="1" parent="1">
      <mxGeometry x="630" y="240" width="510" height="120" as="geometry" />
    </mxCell>

    <!-- Event Bus Interface -->
    <mxCell id="cls_ibus" value="«interface»&lt;br&gt;&lt;b&gt;IEventBus (Message Bus Publisher / Subscriber)&lt;/b&gt;&lt;hr&gt;&lt;p style=&quot;margin:4px; text-align:left;&quot;&gt;+ PublishAsync&amp;lt;TEvent&amp;gt;(@event: TEvent): Task&lt;br&gt;+ Subscribe&amp;lt;TEvent, THandler&amp;gt;(): void&lt;/p&gt;" style="rounded=0;whiteSpace=wrap;html=1;fillColor=#f3e5f5;strokeColor=#7b1fa2;strokeWidth=1.5;verticalAlign=top;spacingTop=4;" vertex="1" parent="1">
      <mxGeometry x="60" y="290" width="450" height="90" as="geometry" />
    </mxCell>

    <!-- Event Contracts -->
    <mxCell id="cls_ev_bkg" value="«DTO / Integration Event»&lt;br&gt;&lt;b&gt;BookingCreatedIntegrationEvent&lt;/b&gt;&lt;hr&gt;&lt;p style=&quot;margin:4px; text-align:left;&quot;&gt;+ BookingId: Guid&lt;br&gt;+ CustomerId: Guid&lt;br&gt;+ CustomerEmail: string&lt;br&gt;+ PetId: Guid&lt;br&gt;+ TotalAmount: decimal&lt;br&gt;+ CheckInDate: DateTime&lt;br&gt;+ CheckOutDate: DateTime&lt;/p&gt;" style="rounded=0;whiteSpace=wrap;html=1;fillColor=#fff8e1;strokeColor=#f57f17;strokeWidth=1.5;verticalAlign=top;spacingTop=4;" vertex="1" parent="1">
      <mxGeometry x="60" y="440" width="260" height="160" as="geometry" />
    </mxCell>

    <mxCell id="cls_ev_pay" value="«DTO / Integration Event»&lt;br&gt;&lt;b&gt;PaymentCompletedIntegrationEvent&lt;/b&gt;&lt;hr&gt;&lt;p style=&quot;margin:4px; text-align:left;&quot;&gt;+ TransactionId: Guid&lt;br&gt;+ BookingId: Guid&lt;br&gt;+ AmountPaid: decimal&lt;br&gt;+ PaymentMethod: string (VNPay)&lt;br&gt;+ Status: PaymentStatusEnum&lt;/p&gt;" style="rounded=0;whiteSpace=wrap;html=1;fillColor=#fff8e1;strokeColor=#f57f17;strokeWidth=1.5;verticalAlign=top;spacingTop=4;" vertex="1" parent="1">
      <mxGeometry x="380" y="440" width="260" height="160" as="geometry" />
    </mxCell>

    <mxCell id="cls_notif_sub" value="«integration handler»&lt;br&gt;&lt;b&gt;SendEmailOnBookingConfirmedHandler&lt;/b&gt;&lt;hr&gt;&lt;p style=&quot;margin:4px; text-align:left;&quot;&gt;+ HandleAsync(@event: PaymentCompletedIntegrationEvent): Task&lt;/p&gt;" style="rounded=0;whiteSpace=wrap;html=1;fillColor=#ffebee;strokeColor=#c62828;strokeWidth=1.5;verticalAlign=top;spacingTop=4;" vertex="1" parent="1">
      <mxGeometry x="710" y="440" width="430" height="70" as="geometry" />
    </mxCell>

    <!-- Connectors -->
    <mxCell id="e_c1" value="«call via gRPC/REST»" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;dashed=1;endArrow=open;strokeWidth=1.5;" edge="1" parent="1" source="cls_ibkg" target="cls_iroom">
      <mxGeometry relative="1" as="geometry" />
    </mxCell>
    <mxCell id="e_c2" value="«call via gRPC/REST»" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;dashed=1;endArrow=open;strokeWidth=1.5;" edge="1" parent="1" source="cls_ibkg" target="cls_ipay">
      <mxGeometry relative="1" as="geometry" />
    </mxCell>
    <mxCell id="e_c3" value="«publishes event»" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;dashed=1;endArrow=open;strokeWidth=1.5;" edge="1" parent="1" source="cls_ibkg" target="cls_ibus">
      <mxGeometry relative="1" as="geometry" />
    </mxCell>
    <mxCell id="e_c4" value="«consumes»" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;dashed=1;endArrow=open;strokeWidth=1.5;" edge="1" parent="1" source="cls_notif_sub" target="cls_ev_pay">
      <mxGeometry relative="1" as="geometry" />
    </mxCell>'''

# 4. VI.2 Deployment Diagram
vi2_deploy = '''    <!-- Title -->
    <mxCell id="sd_title" value="&lt;b&gt;&lt;font style=&quot;font-size: 16px;&quot; color=&quot;#0f3460&quot;&gt;VI.2 Deployment Diagram - Service Discovery Registry with Multi-Instance Scaling&lt;/font&gt;&lt;/b&gt;" style="text;html=1;align=center;verticalAlign=middle;rounded=0;" vertex="1" parent="1">
      <mxGeometry x="250" y="20" width="700" height="30" as="geometry" />
    </mxCell>

    <!-- API Gateway Host -->
    <mxCell id="gw_host" value="«node»&lt;br&gt;&lt;b&gt;API Gateway Host&lt;/b&gt;" style="shape=cube;size=10;whiteSpace=wrap;html=1;boundedLbl=1;fillColor=#fff3e0;strokeColor=#e65100;verticalAlign=top;align=center;spacingTop=16;" vertex="1" parent="1">
      <mxGeometry x="60" y="80" width="260" height="130" as="geometry" />
    </mxCell>
    <mxCell id="gw_comp" value="&lt;b&gt;API Gateway Controller&lt;/b&gt;&lt;br&gt;&lt;font style=&quot;font-size: 11px;&quot; color=&quot;#555555&quot;&gt;• Dynamic Router&lt;br&gt;• Client-Side Load Balancer&lt;br&gt;• Health Query Cache&lt;/font&gt;" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#ffffff;strokeColor=#e65100;" vertex="1" parent="gw_host">
      <mxGeometry x="20" y="42" width="220" height="70" as="geometry" />
    </mxCell>

    <!-- Service Registry (Consul) -->
    <mxCell id="reg_host" value="«cluster node»&lt;br&gt;&lt;b&gt;Service Discovery Cluster (HashiCorp Consul)&lt;/b&gt;" style="shape=cube;size=10;whiteSpace=wrap;html=1;boundedLbl=1;fillColor=#f3e5f5;strokeColor=#7b1fa2;verticalAlign=top;align=center;spacingTop=16;" vertex="1" parent="1">
      <mxGeometry x="460" y="80" width="460" height="130" as="geometry" />
    </mxCell>
    <mxCell id="reg_cat" value="&lt;b&gt;Consul Service Catalog &amp; Health Registry&lt;/b&gt;&lt;br&gt;&lt;font style=&quot;font-size: 10px;&quot; color=&quot;#444444&quot;&gt;• booking-service: [10.0.1.10:5003 (UP), 10.0.1.11:5003 (UP), 10.0.1.12:5003 (UP)]&lt;br&gt;• room-service: [10.0.1.20:5002 (UP), 10.0.1.21:5002 (UP)]&lt;br&gt;• payment-service: [10.0.1.30:5005 (UP)]&lt;/font&gt;" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#ffffff;strokeColor=#7b1fa2;" vertex="1" parent="reg_host">
      <mxGeometry x="20" y="42" width="420" height="70" as="geometry" />
    </mxCell>

    <!-- Step 1 Query -->
    <mxCell id="e_query" value="1. Query Healthy Endpoints" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;strokeColor=#7b1fa2;strokeWidth=2;fontStyle=1;" edge="1" parent="1" source="gw_host" target="reg_host">
      <mxGeometry relative="1" as="geometry" />
    </mxCell>

    <!-- Multi-Instance Workloads Container -->
    <mxCell id="cluster_box" value="Containerized Workloads (Elastic Autoscaling Cluster)" style="swimlane;startSize=25;fontStyle=1;align=center;verticalAlign=top;html=1;whiteSpace=wrap;fillColor=#f8f9fa;strokeColor=#495057;strokeWidth=1.5;" vertex="1" parent="1">
      <mxGeometry x="60" y="270" width="1080" height="420" as="geometry" />
    </mxCell>

    <!-- Booking Instances -->
    <mxCell id="bkg_1" value="«pod #1»&lt;br&gt;&lt;b&gt;BookingService Pod 1&lt;/b&gt;&lt;br&gt;IP: 10.0.1.10:5003&lt;br&gt;Health: /healthz (200 OK)" style="shape=cube;size=8;whiteSpace=wrap;html=1;fillColor=#e8f5e9;strokeColor=#2e7d32;" vertex="1" parent="cluster_box">
      <mxGeometry x="30" y="50" width="220" height="85" as="geometry" />
    </mxCell>
    <mxCell id="bkg_2" value="«pod #2»&lt;br&gt;&lt;b&gt;BookingService Pod 2&lt;/b&gt;&lt;br&gt;IP: 10.0.1.11:5003&lt;br&gt;Health: /healthz (200 OK)" style="shape=cube;size=8;whiteSpace=wrap;html=1;fillColor=#e8f5e9;strokeColor=#2e7d32;" vertex="1" parent="cluster_box">
      <mxGeometry x="280" y="50" width="220" height="85" as="geometry" />
    </mxCell>
    <mxCell id="bkg_3" value="«pod #3 (Autoscaled)»&lt;br&gt;&lt;b&gt;BookingService Pod 3&lt;/b&gt;&lt;br&gt;IP: 10.0.1.12:5003&lt;br&gt;Health: /healthz (200 OK)" style="shape=cube;size=8;whiteSpace=wrap;html=1;fillColor=#e8f5e9;strokeColor=#2e7d32;" vertex="1" parent="cluster_box">
      <mxGeometry x="530" y="50" width="220" height="85" as="geometry" />
    </mxCell>

    <!-- Room Instances -->
    <mxCell id="room_1" value="«pod #1»&lt;br&gt;&lt;b&gt;RoomService Pod 1&lt;/b&gt;&lt;br&gt;IP: 10.0.1.20:5002&lt;br&gt;Health: /healthz (200 OK)" style="shape=cube;size=8;whiteSpace=wrap;html=1;fillColor=#e1f5fe;strokeColor=#0288d1;" vertex="1" parent="cluster_box">
      <mxGeometry x="30" y="170" width="220" height="85" as="geometry" />
    </mxCell>
    <mxCell id="room_2" value="«pod #2»&lt;br&gt;&lt;b&gt;RoomService Pod 2&lt;/b&gt;&lt;br&gt;IP: 10.0.1.21:5002&lt;br&gt;Health: /healthz (200 OK)" style="shape=cube;size=8;whiteSpace=wrap;html=1;fillColor=#e1f5fe;strokeColor=#0288d1;" vertex="1" parent="cluster_box">
      <mxGeometry x="280" y="170" width="220" height="85" as="geometry" />
    </mxCell>

    <!-- Payment Instance -->
    <mxCell id="pay_1" value="«pod #1»&lt;br&gt;&lt;b&gt;PaymentService Pod 1&lt;/b&gt;&lt;br&gt;IP: 10.0.1.30:5005&lt;br&gt;Health: /healthz (200 OK)" style="shape=cube;size=8;whiteSpace=wrap;html=1;fillColor=#fff8e1;strokeColor=#f57f17;" vertex="1" parent="cluster_box">
      <mxGeometry x="30" y="290" width="220" height="85" as="geometry" />
    </mxCell>

    <!-- Auto-register & Heartbeat -->
    <mxCell id="e_heartbeat" value="Auto-Register &amp; Periodic Heartbeat (/healthz)" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;strokeColor=#2e7d32;strokeWidth=1.5;dashed=1;fontStyle=1;" edge="1" parent="1" source="bkg_3" target="reg_host">
      <mxGeometry relative="1" as="geometry">
        <Array as="points">
          <mxPoint x="980" y="360" />
          <mxPoint x="980" y="145" />
        </Array>
      </mxGeometry>
    </mxCell>

    <!-- Forward load-balanced request -->
    <mxCell id="e_forward" value="2. Forward Request (Round-Robin to http://10.0.1.11:5003)" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;strokeColor=#e65100;strokeWidth=2;fontStyle=1;" edge="1" parent="1" source="gw_comp" target="bkg_2">
      <mxGeometry relative="1" as="geometry">
        <Array as="points">
          <mxPoint x="190" y="240" />
          <mxPoint x="450" y="240" />
        </Array>
      </mxGeometry>
    </mxCell>'''

# 5. VI.2 Component Diagram (Fixed with no overlap, large margins, HTML enabled)
vi2_comp = '''    <!-- Title -->
    <mxCell id="sd2_title" value="&lt;b&gt;&lt;font style=&quot;font-size: 16px;&quot; color=&quot;#0f3460&quot;&gt;VI.2 Component Interaction Diagram - Service Discovery &amp; Dynamic Registration&lt;/font&gt;&lt;/b&gt;" style="text;html=1;align=center;verticalAlign=middle;rounded=0;" vertex="1" parent="1">
      <mxGeometry x="250" y="20" width="700" height="30" as="geometry" />
    </mxCell>

    <!-- Service Discovery Server (Top) -->
    <mxCell id="sd_server" value="«infrastructure component»&lt;br&gt;&lt;b style=&quot;font-size: 13px;&quot;&gt;Service Discovery Server (Consul / Eureka)&lt;/b&gt;&lt;hr&gt;&lt;p style=&quot;margin:4px; text-align:left;&quot;&gt;+ RegisterInstance(meta: ServiceRegistrationDTO): void&lt;br&gt;+ DeregisterInstance(instanceId: string): void&lt;br&gt;+ DiscoverServices(serviceName: string): List&amp;lt;ServiceInstanceDTO&amp;gt;&lt;br&gt;+ HealthCheckMonitor(): void&lt;/p&gt;" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#f3e5f5;strokeColor=#7b1fa2;strokeWidth=2;verticalAlign=top;spacingTop=6;" vertex="1" parent="1">
      <mxGeometry x="380" y="70" width="440" height="120" as="geometry" />
    </mxCell>

    <!-- API Gateway (Left) -->
    <mxCell id="sd_gw" value="«component»&lt;br&gt;&lt;b style=&quot;font-size: 13px;&quot;&gt;Dynamic API Gateway&lt;/b&gt;&lt;hr&gt;&lt;p style=&quot;margin:4px; text-align:left;&quot;&gt;+ ResolveTargetEndpoint(logicalRoute: string): Uri&lt;br&gt;+ ApplyLoadBalancing(candidates: List&amp;lt;Uri&amp;gt;): Uri&lt;br&gt;+ ProxyRequestAsync(request: HttpRequest): Task&amp;lt;HttpResponse&amp;gt;&lt;/p&gt;" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#fff3e0;strokeColor=#e65100;strokeWidth=1.5;verticalAlign=top;spacingTop=6;" vertex="1" parent="1">
      <mxGeometry x="50" y="270" width="360" height="110" as="geometry" />
    </mxCell>

    <!-- Booking Service (Right Top) -->
    <mxCell id="sd_bkg" value="«microservice component»&lt;br&gt;&lt;b style=&quot;font-size: 13px;&quot;&gt;BookingService Instance (Self-Registering)&lt;/b&gt;&lt;hr&gt;&lt;p style=&quot;margin:4px; text-align:left;&quot;&gt;+ OnApplicationStarted(): RegisterToDiscovery()&lt;br&gt;+ OnApplicationStopping(): DeregisterFromDiscovery()&lt;br&gt;+ HealthCheckEndpoint(): 200 OK /healthz&lt;br&gt;+ ProcessBookingAsync(dto: CreateBookingDTO): Task&lt;/p&gt;" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#e8f5e9;strokeColor=#2e7d32;strokeWidth=1.5;verticalAlign=top;spacingTop=6;" vertex="1" parent="1">
      <mxGeometry x="560" y="260" width="420" height="120" as="geometry" />
    </mxCell>

    <!-- Room Service (Right Bottom) -->
    <mxCell id="sd_room" value="«microservice component»&lt;br&gt;&lt;b style=&quot;font-size: 13px;&quot;&gt;RoomService Instance (Self-Registering)&lt;/b&gt;&lt;hr&gt;&lt;p style=&quot;margin:4px; text-align:left;&quot;&gt;+ OnApplicationStarted(): RegisterToDiscovery()&lt;br&gt;+ HealthCheckEndpoint(): 200 OK /healthz&lt;br&gt;+ CheckRoomAvailability(dateRange: DateRangeDTO): Task&lt;/p&gt;" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#e1f5fe;strokeColor=#0288d1;strokeWidth=1.5;verticalAlign=top;spacingTop=6;" vertex="1" parent="1">
      <mxGeometry x="560" y="470" width="420" height="110" as="geometry" />
    </mxCell>

    <!-- Connectors -->
    <!-- 1. Booking to Consul (Register) -->
    <mxCell id="e_reg_b" value="1. Register &amp; Periodic Heartbeat" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;entryX=0.8;entryY=1;strokeColor=#2e7d32;strokeWidth=1.5;fontStyle=1;" edge="1" parent="1" source="sd_bkg" target="sd_server">
      <mxGeometry relative="1" as="geometry">
        <Array as="points">
          <mxPoint x="732" y="230" />
          <mxPoint x="732" y="230" />
        </Array>
      </mxGeometry>
    </mxCell>

    <!-- 1b. Room to Consul (Register) -->
    <mxCell id="e_reg_r" value="1. Register &amp; Heartbeat" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;entryX=1;entryY=0.5;strokeColor=#0288d1;strokeWidth=1.5;fontStyle=1;" edge="1" parent="1" source="sd_room" target="sd_server">
      <mxGeometry relative="1" as="geometry">
        <Array as="points">
          <mxPoint x="1030" y="525" />
          <mxPoint x="1030" y="130" />
        </Array>
      </mxGeometry>
    </mxCell>

    <!-- 2. Gateway to Consul (Discover) -->
    <mxCell id="e_disc" value="2. Discover healthy instances&lt;br&gt;(Lookup: &quot;booking-service&quot;)" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;entryX=0;entryY=0.5;strokeColor=#7b1fa2;strokeWidth=1.5;fontStyle=1;" edge="1" parent="1" source="sd_gw" target="sd_server">
      <mxGeometry relative="1" as="geometry">
        <Array as="points">
          <mxPoint x="230" y="130" />
        </Array>
      </mxGeometry>
    </mxCell>

    <!-- 3. Gateway to Booking (Load-Balanced Forwarding) -->
    <mxCell id="e_fwd" value="3. Forward request with&lt;br&gt;Client-Side Load Balancing" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;entryX=0;entryY=0.5;strokeColor=#e65100;strokeWidth=2;fontStyle=1;" edge="1" parent="1" source="sd_gw" target="sd_bkg">
      <mxGeometry relative="1" as="geometry" />
    </mxCell>

    <!-- 4. Booking to Room (Internal RPC) -->
    <mxCell id="e_rpc" value="4. Service-to-Service Discovery &amp; RPC Call&lt;br&gt;(Check Room Availability)" style="edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;entryX=0.5;entryY=0;strokeColor=#333333;strokeWidth=1.5;strokeDasharray=3 3;fontStyle=1;" edge="1" parent="1" source="sd_bkg" target="sd_room">
      <mxGeometry relative="1" as="geometry" />
    </mxCell>'''

with open(r'd:\KI 7\SWD392\PetCareWebsite\docs\diagrams\1_vi1_component.xml', 'w', encoding='utf-8') as f:
    f.write(make_xml(vi1_comp, "VI.1 Component Diagram"))

with open(r'd:\KI 7\SWD392\PetCareWebsite\docs\diagrams\2_vi1_deployment.xml', 'w', encoding='utf-8') as f:
    f.write(make_xml(vi1_deploy, "VI.1 Deployment Diagram"))

with open(r'd:\KI 7\SWD392\PetCareWebsite\docs\diagrams\3_vi1_class.xml', 'w', encoding='utf-8') as f:
    f.write(make_xml(vi1_class, "VI.1 Updated Class Diagram"))

with open(r'd:\KI 7\SWD392\PetCareWebsite\docs\diagrams\4_vi2_deployment.xml', 'w', encoding='utf-8') as f:
    f.write(make_xml(vi2_deploy, "VI.2 Deployment Diagram"))

with open(r'd:\KI 7\SWD392\PetCareWebsite\docs\diagrams\5_vi2_component.xml', 'w', encoding='utf-8') as f:
    f.write(make_xml(vi2_comp, "VI.2 Component Interaction Diagram"))

print("Successfully written 5 individual clean diagram files!")
