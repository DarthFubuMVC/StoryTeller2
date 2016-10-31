import React from 'react';
import Postal from 'postal';
import changes from './../../lib/model/change-commands';
import Icons from './../icons';

const Close = Icons['close'];

module.exports = React.createClass({
  propTypes: {
    step: React.PropTypes.object.isRequired
  },

  render: function(){
    const onclick = e => {
      changes.stepRemoved(this.props.step.parent, this.props.step)

      e.preventDefault();
    }

    return (
      <a
        title="Remove this step or section"
        className="delete"
        onClick={onclick}>
          <Close />
      </a>
    );
  }
});
