var React = require("react");
var {Row, Col} = require('react-bootstrap');

var SpecTitle = require('./spec-title');
var SpecLinks = require('./spec-links');
var SpecCommands = require('./spec-commands');
var LifecycleButton = require('./lifecycle-button');
var SuitePath = require('./../../explorer/suite-path');


function SpecHeader(props){
    // Hokey, but letting it pass
    var headerClass = "";
    if (props.mode == 'editor' && props.spec.active){
        headerClass = "text-primary";
    }

    var lifecycleSpec = props.spec;
    if (props.original){
        lifecycleSpec = props.original;
    }

    return (
        <Row>
            <Col xs={12} md={12}>
                <h3 className={headerClass}>
                    <SuitePath path={props.spec.path} linkToLeaf={true} />
                    <span> / </span>
                    <SpecTitle spec={props.spec} />

                </h3>

                <div>
                    <SpecCommands spec={props.spec}/>
                    <span className="pull-right">
                        <SpecLinks id={props.spec.id} mode={props.mode} spec={props.spec} />

                        <LifecycleButton spec={lifecycleSpec} />
                    </span>
                </div>

                <hr />
            </Col>
        </Row>
    );
}



module.exports = SpecHeader;